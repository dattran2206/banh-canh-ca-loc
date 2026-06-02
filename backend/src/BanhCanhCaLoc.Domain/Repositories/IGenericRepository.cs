using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IGenericRepository<T, TId> where T : class
    {
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAsync(
            System.Linq.Expressions.Expression<System.Func<T, bool>>? predicate = null,
            System.Func<System.Linq.IQueryable<T>, System.Linq.IQueryable<T>>? include = null,
            CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
