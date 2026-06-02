using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.Infrastructure.Repositories
{
    public class TableRepository : GenericRepository<Table, int>, ITableRepository
    {
        public TableRepository(BanhCanhCaLocDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Table>> GetTablesWithAreaAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(t => t.Area)
                .ToListAsync(cancellationToken);
        }
    }
}
