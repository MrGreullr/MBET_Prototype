using MBET.Core.Entities;
using MBET.Core.Entities.Identity;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Entities;
using MBET.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<ISettingsService> _mockSettings;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _mockUserManager = MockUserManager<ApplicationUser>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockSettings = new Mock<ISettingsService>();
            _mockProductRepo = new Mock<IProductRepository>();

            // Default Settings
            _mockSettings.Setup(s => s.GetSettingsAsync())
                .ReturnsAsync(new GlobalSettings
                {
                    DefaultTaxRate = 0.1m,
                    FreeShippingThreshold = 50000
                });

            _service = new OrderService(
                _mockUserManager.Object,
                _mockOrderRepo.Object,
                _mockSettings.Object,
                _mockProductRepo.Object);
        }

        // --- HELPER: Mock UserManager ---
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        // --- 1. ORDER PLACEMENT TESTS ---

        [Fact]
        public async Task PlaceOrder_ShouldDeductStock_AndCreateOrder_WithCorrectMath_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var buyQty = 2;
            var unitPrice = 1000m;
            var shippingFee = 800m;
            var deliveryMethod = "Home";

            var user = new ApplicationUser
            {
                Id = userId,
                ShippingStreet = "123 Main",
                ShippingCity = "Algiers",
                ShippingCountry = "Algeria"
            };
            _mockUserManager.Setup(u => u.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var product = new Product { Id = productId, Title = "GPU", Price = unitPrice, IsActive = true };
            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            // FIX: Setup the new Atomic method to simulate a successful database lock/update
            _mockProductRepo.Setup(r => r.DeductStockAtomicAsync(productId, buyQty)).ReturnsAsync(true);

            var cart = new List<CartItem>
            {
                new CartItem { ProductId = productId, Quantity = buyQty, UnitPrice = unitPrice }
            };

            // Act
            await _service.PlaceOrderAsync(userId, cart, deliveryMethod, shippingFee);

            // Assert
            // FIX: Verify the new Atomic method was called instead of UpdateProductAsync
            _mockProductRepo.Verify(r => r.DeductStockAtomicAsync(productId, buyQty), Times.Once);

            decimal expectedSubtotal = buyQty * unitPrice;
            decimal expectedTax = expectedSubtotal * 0.1m;
            decimal expectedGrandTotal = expectedSubtotal + expectedTax + shippingFee;

            _mockOrderRepo.Verify(r => r.CreateOrderAsync(It.Is<Order>(o =>
                o.Subtotal == expectedSubtotal &&
                o.TaxAmount == expectedTax &&
                o.ShippingFee == shippingFee &&
                o.DeliveryMethod == deliveryMethod &&
                o.GrandTotal == expectedGrandTotal
            )), Times.Once);
        }

        [Fact]
        public async Task PlaceOrder_ShouldFail_WhenAddressMissing()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, ShippingStreet = "" };
            _mockUserManager.Setup(u => u.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var cart = new List<CartItem> { new CartItem { ProductId = Guid.NewGuid(), Quantity = 1 } };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.PlaceOrderAsync(userId, cart, "Pickup", 400m));
        }

        [Fact]
        public async Task PlaceOrder_ShouldFail_WhenStockInsufficient()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var user = new ApplicationUser { Id = userId, ShippingStreet = "St", ShippingCity = "Ct", ShippingCountry = "Cn" };
            _mockUserManager.Setup(u => u.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var product = new Product { Id = productId, Title = "GPU", IsActive = true };
            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            // FIX: Simulate the database rejecting the update because stock dipped below 0
            _mockProductRepo.Setup(r => r.DeductStockAtomicAsync(productId, 5)).ReturnsAsync(false);

            var cart = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 5 } };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.PlaceOrderAsync(userId, cart, "Home", 800m));
            Assert.Contains("Insufficient stock", ex.Message);
        }

        // --- 2. ORDER MANAGEMENT & CANCELLATION TESTS ---

        [Fact]
        public async Task UpdateStatus_ShouldRestoreStock_WhenCancelled()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var qty = 3;

            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Processing,
                Items = new List<OrderItem> { new OrderItem { ProductId = productId, Quantity = qty } }
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            await _service.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);

            // Assert
            // FIX: Verify the Atomic Restoration
            _mockProductRepo.Verify(r => r.RestoreStockAtomicAsync(productId, qty), Times.Once);
            Assert.Equal(OrderStatus.Cancelled, order.Status);
            _mockOrderRepo.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldNotRestoreStock_WhenAlreadyCancelled()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, Status = OrderStatus.Cancelled };
            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            await _service.UpdateOrderStatusAsync(orderId, OrderStatus.Returned);

            // FIX: Verify no atomic restoration occurred
            _mockProductRepo.Verify(r => r.RestoreStockAtomicAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }

        // --- 3. DELETION PIPELINE TESTS ---

        [Fact]
        public async Task PrepareForDeletion_ShouldRestoreStock_IfOrderIsActive()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem> { new OrderItem { ProductId = productId, Quantity = 2 } }
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            await _service.PrepareOrderForDeletionAsync(orderId);

            // Assert
            // FIX: Verify atomic restoration
            _mockProductRepo.Verify(r => r.RestoreStockAtomicAsync(productId, 2), Times.Once);
        }

        [Fact]
        public async Task PrepareForDeletion_ShouldNotRestoreStock_IfAlreadyCancelled()
        {
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Cancelled,
                Items = new List<OrderItem> { new OrderItem { ProductId = Guid.NewGuid(), Quantity = 2 } }
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            await _service.PrepareOrderForDeletionAsync(orderId);

            _mockProductRepo.Verify(r => r.RestoreStockAtomicAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }
    }
}