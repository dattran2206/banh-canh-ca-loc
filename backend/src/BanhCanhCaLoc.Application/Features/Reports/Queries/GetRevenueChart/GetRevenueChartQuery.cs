using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Reports.Queries.GetRevenueChart
{
    public record RevenueChartPoint(string Date, decimal Revenue);

    public record GetRevenueChartQuery() : IQuery<Result<IReadOnlyList<RevenueChartPoint>>>;

    public class GetRevenueChartQueryHandler : IQueryHandler<GetRevenueChartQuery, Result<IReadOnlyList<RevenueChartPoint>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRevenueChartQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<RevenueChartPoint>>> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6);
            var payments = await _unitOfWork.Repository<Payment, Guid>()
                .GetAsync(p => p.PaidAt.Date >= startDate, cancellationToken);

            var chartData = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .Select(date => new RevenueChartPoint(
                    date.ToString("dd/MM"),
                    payments.Where(p => p.PaidAt.Date == date).Sum(p => p.TotalAmount)
                ))
                .ToList();

            return chartData;
        }
    }
}
