using Exam.Domain;
using Exam.Domain.Entities.Common;
using Exam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam.Infrastructure.Repositories
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
            Expression<Func<TEntity, bool>> predicate)
        {
            IQueryable<TEntity> query = _dbSet;

            // 🔥 فلترة Soft Delete
            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            return await query
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        // ============================================
        // ADD
        // ============================================
        public async Task<int> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        // ============================================
        // UPDATE
        // ============================================
        public async Task<int> UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return await _context.SaveChangesAsync();
        }

        // ============================================
        // DELETE (Soft Delete Support)
        // ============================================
        public async Task<int> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity == null)
                return 0;

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

            return await _context.SaveChangesAsync();
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
    }
}