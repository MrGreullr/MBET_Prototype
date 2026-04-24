using MBET.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<Guid> CreateOrderAsync(Order order);
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task UpdateOrderAsync(Order order);

        // ADDED: Explicit delete method to avoid generic casting issues
        Task DeleteOrderAsync(Guid id);
    }
}