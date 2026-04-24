using MBET.Core.Entities;
using MBET.Infrastructure.Services;
using MBET.Infrastructure.Entities;
using System;
using System.Linq;
using Xunit;

namespace MBET.Tests.Services
{
    public class CartServiceTests
    {
        [Fact]
        public void AddToCart_ShouldIncreaseQuantity_WhenItemExists_AndStockAvailable()
        {
            // Arrange
            var service = new CartService();
            // Ensure product is Visible and has Stock
            var product = new Product { Id = Guid.NewGuid(), Title = "Test Product", Price = 100, StockQuantity = 10, IsVisible = true };

            // Act
            bool result1 = service.AddToCart(product);
            bool result2 = service.AddToCart(product); // Add same item twice

            // Assert
            Assert.True(result1, "First add should succeed");
            Assert.True(result2, "Second add should succeed");

            var items = service.GetItems();
            Assert.Single(items);
            Assert.Equal(2, items.First().Quantity);
            Assert.Equal(200, service.GetTotal());
        }

        [Fact]
        public void AddToCart_ShouldFail_WhenHidden()
        {
            // Arrange
            var service = new CartService();
            var product = new Product { Id = Guid.NewGuid(), Title = "Hidden Item", IsVisible = false, StockQuantity = 10 };

            // Act
            bool result = service.AddToCart(product);

            // Assert
            Assert.False(result, "Should not add hidden product");
            Assert.Empty(service.GetItems());
        }

        [Fact]
        public void AddToCart_ShouldFail_WhenOutOfStock_QuantityZero()
        {
            // Arrange
            var service = new CartService();
            var product = new Product { Id = Guid.NewGuid(), Title = "OOS Item", IsVisible = true, StockQuantity = 0 };

            // Act
            bool result = service.AddToCart(product);

            // Assert
            Assert.False(result, "Should not add OOS product");
            Assert.Empty(service.GetItems());
        }

        [Fact]
        public void AddToCart_ShouldFail_WhenManualOutOfStock()
        {
            // Arrange
            var service = new CartService();
            // Has quantity, but manually marked OOS
            var product = new Product { Id = Guid.NewGuid(), Title = "Manual OOS", IsVisible = true, StockQuantity = 10, IsOutOfStock = true };

            // Act
            bool result = service.AddToCart(product);

            // Assert
            Assert.False(result, "Should not add manually OOS product");
            Assert.Empty(service.GetItems());
        }

        [Fact]
        public void AddToCart_ShouldFail_WhenExceedingStock()
        {
            // Arrange
            var service = new CartService();
            var product = new Product { Id = Guid.NewGuid(), Title = "Low Stock Item", IsVisible = true, StockQuantity = 1 };

            // Act
            bool result1 = service.AddToCart(product); // OK (1/1)
            bool result2 = service.AddToCart(product); // Fail (2/1)

            // Assert
            Assert.True(result1);
            Assert.False(result2, "Should not allow adding beyond stock limit");
            Assert.Equal(1, service.GetItems().First().Quantity);
        }

        [Fact]
        public void RemoveFromCart_ShouldRemoveItem_WhenExists()
        {
            // Arrange
            var service = new CartService();
            var p1 = new Product { Id = Guid.NewGuid(), IsVisible = true, StockQuantity = 5 };
            service.AddToCart(p1);

            // Act
            service.RemoveFromCart(p1.Id);

            // Assert
            Assert.Empty(service.GetItems());
        }
    }
}