using Exam.Domain;
using Exam.Domain.Entities.Common;
using Exam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam.Infrastructure.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        // ============================================
        // GET ALL
        // ============================================
        public async Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = true)
        {
            IQueryable<TEntity> query = _dbSet;

            // 🔥 فلترة Soft Delete تلقائيًا
            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        // ============================================
        // GET BY ID
        // ============================================
        public async Task<TEntity?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            // 🔥 منع إرجاع عنصر محذوف منطقيًا
            if (entity is BaseEntity baseEntity && baseEntity.IsDeleted)
                return null;

            return entity;
        }

        // ============================================
        // FIND WITH CONDITION
        // ============================================
        public async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            params string[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            // 🔥 Soft Delete
            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        // ============================================
        // ADD
        // ============================================
        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // ============================================
        // UPDATE
        // ============================================
        public Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        // ============================================
        // DELETE (Soft Delete Support)
        // ============================================
        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity == null)
                return;

            if (entity is BaseEntity baseEntity)
            {
                // 🔥 Soft Delete
                baseEntity.IsDeleted = true;
                _dbSet.Update(entity);
            }
            else
            {
                // 🔥 Hard Delete
                _dbSet.Remove(entity);
            }
        }

        // ============================================
        // EXISTS
        // ============================================
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet
                .Where(e => EF.Property<int>(e, "Id") == id)
                .AnyAsync();
        }

        // ============================================
        // GET PAGED
        // ============================================
        public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            Expression<Func<TEntity, bool>>? predicate = null, 
            bool asNoTracking = true)
        {
            IQueryable<TEntity> query = _dbSet;

            // 🔥 Soft Delete filter
            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            int totalCount = await query.CountAsync();

            if (asNoTracking)
                query = query.AsNoTracking();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}