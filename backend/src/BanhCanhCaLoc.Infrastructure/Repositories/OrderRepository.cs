using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(BanhCanhCaLocDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetWithItemsAndTableAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(o => o.Table)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync(string? status, CancellationToken cancellationToken = default)
        {
            var query = DbSet
                .Include(o => o.Table)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                {
                    query = query.Where(o => o.Status != "paid");
                }
                else
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var countToday = await DbSet
                .CountAsync(o => o.CreatedAt.Date == today, cancellationToken);
            return $"OD-{(countToday + 1):D3}";
        }
    }
}
