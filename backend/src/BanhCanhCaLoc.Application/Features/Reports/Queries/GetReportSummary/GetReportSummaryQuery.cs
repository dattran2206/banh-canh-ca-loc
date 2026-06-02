using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Reports.Queries.GetReportSummary
{
    public record TopItemSummary(string Name, int Qty);
    public record DailyChartPoint(string Name, decimal Revenue, int Bills);

    public record ReportSummary(
        decimal TodayRevenue,
        decimal YesterdayRevenue,
        int TodayBills,
        int YesterdayBills,
        decimal ThisWeekRevenue,
        decimal LastWeekRevenue,
        int ThisWeekBills,
        int LastWeekBills,
        decimal ThisMonthRevenue,
        decimal LastMonthRevenue,
        int ThisMonthBills,
        int LastMonthBills,
        IReadOnlyList<TopItemSummary> TopItems,
        IReadOnlyList<DailyChartPoint> DailyChart
    );

    public record GetReportSummaryQuery() : IQuery<Result<ReportSummary>>;

    public class GetReportSummaryQueryHandler : IQueryHandler<GetReportSummaryQuery, Result<ReportSummary>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetReportSummaryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ReportSummary>> Handle(GetReportSummaryQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);

            var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var thisWeekStart = today.AddDays(-diff);
            var thisWeekEnd = thisWeekStart.AddDays(7);
            var lastWeekStart = thisWeekStart.AddDays(-7);
            var lastWeekEnd = thisWeekStart;

            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart;

            var payments = await _unitOfWork.Repository<Payment, Guid>().GetAllAsync(cancellationToken);

            var todayPayments = payments.Where(p => p.PaidAt.Date == today).ToList();
            var yesterdayPayments = payments.Where(p => p.PaidAt.Date == yesterday).ToList();

            var thisWeekPayments = payments.Where(p => p.PaidAt.Date >= thisWeekStart && p.PaidAt.Date < thisWeekEnd).ToList();
            var lastWeekPayments = payments.Where(p => p.PaidAt.Date >= lastWeekStart && p.PaidAt.Date < lastWeekEnd).ToList();

            var thisMonthPayments = payments.Where(p => p.PaidAt.Date >= thisMonthStart && p.PaidAt.Date < thisMonthEnd).ToList();
            var lastMonthPayments = payments.Where(p => p.PaidAt.Date >= lastMonthStart && p.PaidAt.Date < lastMonthEnd).ToList();

            var orderItems = await _unitOfWork.Repository<OrderItem, Guid>()
                .GetAsync(oi => oi.Order != null && oi.Order.Status == "paid", cancellationToken);

            var menuItems = await _unitOfWork.MenuItems.GetAllAsync(cancellationToken);
            var menuItemMap = menuItems.ToDictionary(m => m.Id);

            var topItemsGrouped = orderItems
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new TopItemSummary(
                    menuItemMap.TryGetValue(g.Key, out var menu) ? menu.Name : "Unknown",
                    g.Sum(oi => oi.Quantity)
                ))
                .OrderByDescending(g => g.Qty)
                .Take(5)
                .ToList();

            var dailyChartList = new List<DailyChartPoint>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayPayments = payments.Where(p => p.PaidAt.Date == date).ToList();
                dailyChartList.Add(new DailyChartPoint(
                    date.ToString("dd/MM"),
                    dayPayments.Sum(p => p.TotalAmount),
                    dayPayments.Count
                ));
            }

            return new ReportSummary(
                todayPayments.Sum(p => p.TotalAmount),
                yesterdayPayments.Sum(p => p.TotalAmount),
                todayPayments.Count,
                yesterdayPayments.Count,
                thisWeekPayments.Sum(p => p.TotalAmount),
                lastWeekPayments.Sum(p => p.TotalAmount),
                thisWeekPayments.Count,
                lastWeekPayments.Count,
                thisMonthPayments.Sum(p => p.TotalAmount),
                lastMonthPayments.Sum(p => p.TotalAmount),
                thisMonthPayments.Count,
                lastMonthPayments.Count,
                topItemsGrouped,
                dailyChartList
            );
        }
    }
}
