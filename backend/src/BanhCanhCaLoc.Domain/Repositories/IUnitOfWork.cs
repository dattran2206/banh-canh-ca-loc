using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IMenuItemRepository MenuItems { get; }
        ITableRepository Tables { get; }
        IUserRepository Users { get; }
        IIngredientRepository Ingredients { get; }

        IGenericRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
