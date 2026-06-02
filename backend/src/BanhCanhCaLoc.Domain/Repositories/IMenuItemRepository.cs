using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IMenuItemRepository : IGenericRepository<MenuItem, int>
    {
        Task<MenuItem?> GetWithRecipeAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MenuItem>> GetMenuItemsWithDetailsAsync(CancellationToken cancellationToken = default);
    }
}
