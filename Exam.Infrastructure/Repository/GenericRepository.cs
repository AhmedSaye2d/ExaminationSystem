using Exam.Domain;
using Exam.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Repository
{
    public class GenericRepository<TEntity> : IGeneric<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<int> AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            return 1; // Assuming SaveChanges is handled by UnitOfWork
        }

        public async Task<int> UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            return 1;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<TEntity>().Remove(entity);
                return 1;
            }
            return 0;
        }
    }
}
