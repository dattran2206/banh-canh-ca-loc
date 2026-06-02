using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Reports.Queries.GetDashboardStats
{
    public record DashboardStats(
        decimal TodayRevenue,
        int TodayBills,
        int OccupiedCount,
        int TotalTables,
        int ActiveOrderCount,
        IReadOnlyList<Ingredient> LowStock
    );

    public record GetDashboardStatsQuery() : IQuery<Result<DashboardStats>>;

    public class GetDashboardStatsQueryHandler : IQueryHandler<GetDashboardStatsQuery, Result<DashboardStats>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DashboardStats>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var todayUtc = DateTime.UtcNow.Date;

            var todayPayments = await _unitOfWork.Repository<Payment, Guid>()
                .GetAsync(p => p.PaidAt.Date == todayUtc, cancellationToken);

            var todayRevenue = todayPayments.Sum(p => p.TotalAmount);
            var todayBills = todayPayments.Count;

            var activeOrders = await _unitOfWork.Orders
                .GetAsync(o => o.Status != "paid", cancellationToken);

            var occupiedTableIds = activeOrders.Select(o => o.TableId).Distinct().ToList();
            var occupiedCount = occupiedTableIds.Count;

            var tables = await _unitOfWork.Tables.GetAllAsync(cancellationToken);
            var totalTables = tables.Count;
            var activeOrderCount = activeOrders.Count;

            var lowStock = await _unitOfWork.Ingredients
                .GetAsync(i => i.CurrentStock <= i.MinThreshold, cancellationToken);

            return new DashboardStats(
                todayRevenue,
                todayBills,
                occupiedCount,
                totalTables,
                activeOrderCount,
                lowStock
            );
        }
    }
}
