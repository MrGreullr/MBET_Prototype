using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Entities;
using MBET.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly ISettingsService _settingsService;
        private readonly IProductRepository _productRepository;

        public OrderService(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository,
            ISettingsService settingsService,
            IProductRepository productRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
            _settingsService = settingsService;
            _productRepository = productRepository;
        }

        public async Task<Guid> PlaceOrderAsync(Guid userId, List<CartItem> cartItems, string deliveryMethod, decimal shippingFee)
        {
            if (cartItems == null || !cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found.");

            if (user.IsBanned)
                throw new UnauthorizedAccessException("Account is restricted.");

            if (string.IsNullOrWhiteSpace(user.ShippingStreet) ||
                string.IsNullOrWhiteSpace(user.ShippingCity) ||
                string.IsNullOrWhiteSpace(user.ShippingCountry))
            {
                throw new InvalidOperationException("MissingShippingAddress");
            }

            var settings = await _settingsService.GetSettingsAsync();

            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;

            foreach (var cartItem in cartItems)
            {
                // We still fetch the product to verify it exists and get its metadata (like Title)
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

                if (product == null || !product.IsActive)
                    throw new Exception($"Product unavailable.");

                // 🔥 THE FIX: Atomic Stock Deduction directly on the DB 🔥
                var success = await _productRepository.DeductStockAtomicAsync(product.Id, cartItem.Quantity);

                if (!success)
                {
                    // If rowsAffected was 0, it means another thread bought the last item right before us!
                    throw new Exception($"Insufficient stock for '{product.Title}'.");
                }

                subtotal += cartItem.UnitPrice * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity,
                });
            }

            var taxAmount = subtotal * settings.DefaultTaxRate;
            var grandTotal = subtotal + taxAmount + shippingFee;

            var order = new Order
            {
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                CustomerName = $"{user.FirstName} {user.LastName}",
                CustomerPhone = user.PhoneNumber,
                ShippingAddress = $"{user.ShippingStreet}, {user.ShippingCity}, {user.ShippingCountry} {user.ShippingZipCode}".Trim(),
                DeliveryMethod = deliveryMethod,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                ShippingFee = shippingFee,
                GrandTotal = grandTotal,
                Items = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            return await _orderRepository.CreateOrderAsync(order);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found.");

            var oldStatus = order.Status;

            if (oldStatus == OrderStatus.Delivered && newStatus == OrderStatus.Pending)
                throw new InvalidOperationException("Cannot revert a Delivered order to Pending.");

            // 1. Stock Restoration Logic
            if ((newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Returned) &&
                (oldStatus != OrderStatus.Cancelled && oldStatus != OrderStatus.Returned))
            {
                await RestoreStockForOrder(order);
            }

            // 2. Update Status
            order.Status = newStatus;

            // 3. Payment Status Logic
            if (newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Returned)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }
            else if (newStatus == OrderStatus.Shipped || newStatus == OrderStatus.Delivered)
            {
                if (order.PaymentStatus == PaymentStatus.Unpaid)
                    order.PaymentStatus = PaymentStatus.Paid;
            }

            // CRITICAL: Prevent EF Core from saving "stale" product data attached to the order items.
            if (order.Items != null)
            {
                foreach (var item in order.Items) item.Product = null;
            }

            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            if (order.Items != null)
            {
                foreach (var item in order.Items) item.Product = null;
            }
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task PrepareOrderForDeletionAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return;

            if (order.Status != OrderStatus.Cancelled && order.Status != OrderStatus.Returned)
            {
                await RestoreStockForOrder(order);
            }
        }

        // --- Helper Methods ---

        private async Task RestoreStockForOrder(Order order)
        {
            if (order.Items != null && order.Items.Any())
            {
                foreach (var item in order.Items)
                {
                    // 🔥 THE FIX: Atomic Stock Restoration 🔥
                    await _productRepository.RestoreStockAtomicAsync(item.ProductId, item.Quantity);
                }
            }
        }
    }
}