using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.Infrastructure.Repositories
{
    public class MenuItemRepository : GenericRepository<MenuItem, int>, IMenuItemRepository
    {
        public MenuItemRepository(BanhCanhCaLocDbContext context) : base(context)
        {
        }

        public async Task<MenuItem?> GetWithRecipeAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<MenuItem>> GetMenuItemsWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(m => m.Category)
                .ToListAsync(cancellationToken);
        }
    }
}
