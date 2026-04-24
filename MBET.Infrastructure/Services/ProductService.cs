using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetFreshDropsAsync(int count, ProductDisplayMode mode)
        {
            // Business Logic: We could enforce max counts, cache results, or map to DTOs here.
            // For now, we pass through to the specific repo query.
            return await _repository.GetLandingProductsAsync(count, mode);
        }

        public async Task<PagedResult<Product>> GetCatalogItemsAsync(string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            return await _repository.GetProductsAsync(search, categoryId, minPrice, maxPrice, page, pageSize);
        }
    }
}