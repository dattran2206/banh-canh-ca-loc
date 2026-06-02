using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BanhCanhCaLocDbContext _context;
        private readonly ConcurrentDictionary<string, object> _repositories = new();

        public UnitOfWork(
            BanhCanhCaLocDbContext context,
            IOrderRepository orders,
            IMenuItemRepository menuItems,
            ITableRepository tables,
            IUserRepository users,
            IIngredientRepository ingredients)
        {
            _context = context;
            Orders = orders;
            MenuItems = menuItems;
            Tables = tables;
            Users = users;
            Ingredients = ingredients;
        }

        public IOrderRepository Orders { get; }
        public IMenuItemRepository MenuItems { get; }
        public ITableRepository Tables { get; }
        public IUserRepository Users { get; }
        public IIngredientRepository Ingredients { get; }

        public IGenericRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class
        {
            var typeName = typeof(TEntity).Name;

            return (IGenericRepository<TEntity, TId>)_repositories.GetOrAdd(typeName, _ =>
            {
                return new GenericRepository<TEntity, TId>(_context);
            });
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
