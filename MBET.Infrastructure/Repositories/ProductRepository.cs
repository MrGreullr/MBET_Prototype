using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Core.Models;
using MBET.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;

        public ProductRepository(IDbContextFactory<MBETDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm = null,
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageNumber = 1,
            int pageSize = 12)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive) // FIX: Filter out soft-deleted items
                .AsNoTracking();

            // 1. Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // 2. Count Total (before paging)
            var totalCount = await query.CountAsync();

            // 3. Paging
            var items = await query
                .OrderByDescending(p => p.IsPriority)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 4)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsPriority && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Category>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive); // Ensure we don't get deleted items by ID
        }

        // Admin Actions
        public async Task<Guid> AddProductAsync(Product product)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateProductAsync(Product product)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Products.Update(product);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft Delete
                product.IsActive = false;
                context.Products.Update(product);
                await context.SaveChangesAsync();
            }
        }

        // --- NEW: Landing Page Engine ---
        public async Task<IEnumerable<Product>> GetLandingProductsAsync(int count, ProductDisplayMode mode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.IsVisible) // Only show active & visible products
                .AsNoTracking();

            // Apply Sorting Strategy
            switch (mode)
            {
                case ProductDisplayMode.Priority:
                    // Show marked Priority items first, then fill remaining slots with newest
                    query = query.OrderByDescending(p => p.IsPriority)
                                 .ThenByDescending(p => p.CreatedAt);
                    break;

                case ProductDisplayMode.Newest:
                    // Standard Fresh Drops
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;

                case ProductDisplayMode.Random:
                    // Random shuffle (EF Core translates Guid.NewGuid() to random db function)
                    query = query.OrderBy(p => Guid.NewGuid());
                    break;
            }

            return await query.Take(count).ToListAsync();
        }
        public async Task<bool> DeductStockAtomicAsync(Guid productId, int quantity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // ExecuteUpdateAsync sends an immediate, atomic SQL UPDATE statement.
            // The condition (StockQuantity >= quantity) ensures we never dip below 0 concurrently.
            var rowsAffected = await context.Products
                .Where(p => p.Id == productId && p.StockQuantity >= quantity)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity)
                    .SetProperty(p => p.IsOutOfStock, p => (p.StockQuantity - quantity) <= 0));

            return rowsAffected > 0;
        }

        public async Task<bool> RestoreStockAtomicAsync(Guid productId, int quantity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var rowsAffected = await context.Products
                .Where(p => p.Id == productId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.StockQuantity, p => p.StockQuantity + quantity)
                    .SetProperty(p => p.IsOutOfStock, p => false));

            return rowsAffected > 0;
        }
    }
}