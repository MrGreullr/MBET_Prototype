using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace MBET.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        /// <summary>
        /// Performs a deletion based on the implementation (typically Soft Delete via context interception or flag setting).
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Permanently removes the record from the database using high-performance ExecuteDeleteAsync.
        /// </summary>
        Task<bool> HardDeleteAsync(Guid id);
    }
}
