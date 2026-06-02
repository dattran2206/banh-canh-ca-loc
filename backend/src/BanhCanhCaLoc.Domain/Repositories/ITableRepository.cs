using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface ITableRepository : IGenericRepository<Table, int>
    {
        Task<IReadOnlyList<Table>> GetTablesWithAreaAsync(CancellationToken cancellationToken = default);
    }
}
