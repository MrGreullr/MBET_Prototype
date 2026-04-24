using MBET.Core.Entities;
using MBET.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MBET.Infrastructure.Entities;

namespace MBET.Infrastructure.Services
{
    // Register as Scoped in Program.cs
    public class CartService
    {
        private List<CartItem> _cartItems = new();

        public event Action? OnChange;

        public bool AddToCart(Product product)
        {
            // 1. Validation: Visibility & Stock
            if (!product.IsVisible) return false;
            if (product.IsOutOfStock || product.StockQuantity <= 0) return false;

            var existing = _cartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing != null)
            {
                // Check if adding one more exceeds stock
                if (existing.Quantity + 1 > product.StockQuantity) return false;
                existing.Quantity++;
            }
            else
            {
                _cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                });
            }
            NotifyStateChanged();
            return true;
        }

        public void DecreaseQuantity(Guid productId)
        {
            var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                // We do NOT remove if it hits 0 here; explicit removal is separate
                NotifyStateChanged();
            }
        }

        public bool UpdateQuantity(Guid productId, int newQuantity)
        {
            var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (newQuantity <= 0) return false; // Or handle removal

                // Validate against stock
                if (item.Product != null && newQuantity > item.Product.StockQuantity)
                {
                    return false; // Stock limit reached
                }

                item.Quantity = newQuantity;
                NotifyStateChanged();
                return true;
            }
            return false;
        }

        public void RemoveFromCart(Guid productId)
        {
            var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
                NotifyStateChanged();
            }
        }

        public List<CartItem> GetItems() => _cartItems;

        public decimal GetTotal() => _cartItems.Sum(x => x.UnitPrice * x.Quantity);

        public void Clear()
        {
            _cartItems.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}