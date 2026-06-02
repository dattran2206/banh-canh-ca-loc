using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order, Guid>
    {
        Task<Order?> GetWithItemsAndTableAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync(string? status, CancellationToken cancellationToken = default);
        Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
    }
}
