using MBET.Core.Entities;
using MBET.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for product data operations.
    /// Repository Pattern ensures the Web layer never calls the DB directly.
    /// </summary>
    public interface IProductRepository
    {
        Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm = null,
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageNumber = 1,
            int pageSize = 12);

        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 4);

        // CRUD for Phase 5 Admin
        Task<Guid> AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid id);

        Task<IEnumerable<Product>> GetLandingProductsAsync(int count, ProductDisplayMode mode);

        // --- NEW: ATOMIC DATABASE OPERATIONS ---
        Task<bool> DeductStockAtomicAsync(Guid productId, int quantity);
        Task<bool> RestoreStockAtomicAsync(Guid productId, int quantity);
    }
}