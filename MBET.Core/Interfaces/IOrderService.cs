using MBET.Core.Entities;
using MBET.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Guid> PlaceOrderAsync(Guid userId, List<CartItem> cartItems, string deliveryMethod, decimal shippingFee);  
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task UpdateOrderAsync(Order order);

        /// <summary>
        /// Called before an order is deleted to restore stock to the inventory.
        /// </summary>
        Task PrepareOrderForDeletionAsync(Guid orderId);
    }
}