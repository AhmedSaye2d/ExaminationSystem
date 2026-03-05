using System;
using System.Threading.Tasks;
using Exam.Domain;

namespace Exam.Domain.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();
    }
}
