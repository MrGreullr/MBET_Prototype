using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;

        public OrderRepository(IDbContextFactory<MBETDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Guid> CreateOrderAsync(Order order)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order.Id;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Orders.Update(order);
            await context.SaveChangesAsync();
        }

        // ADDED: Explicit implementation for the Service to use
        public async Task DeleteOrderAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }
        }
    }
}