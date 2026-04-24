using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace MBET.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        // We use the Factory to ensure a fresh context for every operation
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;

        public Repository(IDbContextFactory<MBETDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Standard Delete - Usually triggers Soft Delete if the Entity implements ISoftDelete
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);

            if (entity == null) return false;

            // Strategy 1: Explicitly set flag if interface exists
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = DateTimeOffset.UtcNow;
                context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                // Strategy 2: Fallback to standard remove (Context might handle interception)
                context.Set<T>().Remove(entity);
            }

            var result = await context.SaveChangesAsync();

            // TODO: Inject CacheInvalidator here if caching is added later
            // await _invalidator.InvalidateItemAsync<T>(id);

            return result > 0;
        }

        /// <summary>
        /// High-performance Hard Delete
        /// </summary>
        public async Task<bool> HardDeleteAsync(Guid id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Note: EF Core 7+ ExecuteDeleteAsync
            // We assume the entity has an 'Id' property. 
            // In a purely generic class without a base class constraint, we rely on EF's shadow properties 
            // or we must assume T : BaseEntity. For this snippet, we assume standard EF behaviour.

            // Since T is generic, we can't use lambda x => x.Id == id easily without a constraint.
            // We find first to ensure it exists, then delete. 
            // For true bulk performance, T should inherit from a BaseEntity with Id.

            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null) return false;

            context.Set<T>().Remove(entity);
            var result = await context.SaveChangesAsync();

            // TODO: Inject CacheInvalidator here if caching is added later

            return result > 0;
        }
    }
}
