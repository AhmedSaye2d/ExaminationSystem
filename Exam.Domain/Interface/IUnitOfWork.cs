using System;
using System.Threading.Tasks;

namespace Exam.Domain.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IGeneric<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();
    }
}
