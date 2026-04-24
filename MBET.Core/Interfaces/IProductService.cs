using MBET.Core.Entities;
using MBET.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Retrieves products for the "Fresh Drops" section based on global settings.
        /// </summary>
        Task<IEnumerable<Product>> GetFreshDropsAsync(int count, ProductDisplayMode mode);

        /// <summary>
        /// Main Catalog retrieval with filtering.
        /// </summary>
        Task<PagedResult<Product>> GetCatalogItemsAsync(string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
    }
}