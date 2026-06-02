using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BanhCanhCaLoc.Api.Data;
using BanhCanhCaLoc.Api.Models;
using System.Security.Claims;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public ReportsController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var todayUtc = DateTime.UtcNow.Date;

            // Get payments today
            var todayPayments = await _context.Payments
                .Where(p => p.PaidAt.Date == todayUtc)
                .ToListAsync();

            var todayRevenue = todayPayments.Sum(p => p.TotalAmount);
            var todayBills = todayPayments.Count;

            // Get occupied tables count
            var activeOrders = await _context.Orders
                .Where(o => o.Status != "paid")
                .ToListAsync();

            var occupiedTableIds = activeOrders.Select(o => o.TableId).Distinct().ToList();
            var occupiedCount = occupiedTableIds.Count;

            var totalTables = await _context.Tables.CountAsync();
            var activeOrderCount = activeOrders.Count;

            // Low stock alerts
            var lowStock = await _context.Ingredients
                .Where(i => i.CurrentStock <= i.MinThreshold)
                .ToListAsync();

            return Ok(new
            {
                todayRevenue,
                todayBills,
                occupiedCount,
                totalTables,
                activeOrderCount,
                lowStock
            });
        }

        [HttpGet("activity-logs")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetActivityLogs()
        {
            var logs = await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(100) // limit to 100 entries
                .Select(l => new
                {
                    l.Id,
                    l.UserId,
                    UserFullName = l.User != null ? l.User.FullName : "Hệ thống",
                    l.Action,
                    l.Detail,
                    l.Timestamp
                })
                .ToListAsync();

            return Ok(logs);
        }

        public class CreateLogDto
        {
            public string Action { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
        }

        [HttpPost("activity-logs")]
        public async Task<IActionResult> AddActivityLog([FromBody] CreateLogDto dto)
        {
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = dto.Action,
                Detail = dto.Detail,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("revenue-chart")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetRevenueChartData()
        {
            // Summarize last 7 days revenue
            var startDate = DateTime.UtcNow.Date.AddDays(-6);
            var payments = await _context.Payments
                .Where(p => p.PaidAt.Date >= startDate)
                .ToListAsync();

            var chartData = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .Select(date => new
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = payments.Where(p => p.PaidAt.Date == date).Sum(p => p.TotalAmount)
                })
                .ToList();

            return Ok(chartData);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetReportSummary()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);

            // Week boundaries (Monday to Sunday)
            var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var thisWeekStart = today.AddDays(-diff);
            var thisWeekEnd = thisWeekStart.AddDays(7);
            var lastWeekStart = thisWeekStart.AddDays(-7);
            var lastWeekEnd = thisWeekStart;

            // Month boundaries
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart;

            var payments = await _context.Payments.ToListAsync();

            var todayPayments = payments.Where(p => p.PaidAt.Date == today).ToList();
            var yesterdayPayments = payments.Where(p => p.PaidAt.Date == yesterday).ToList();

            var thisWeekPayments = payments.Where(p => p.PaidAt.Date >= thisWeekStart && p.PaidAt.Date < thisWeekEnd).ToList();
            var lastWeekPayments = payments.Where(p => p.PaidAt.Date >= lastWeekStart && p.PaidAt.Date < lastWeekEnd).ToList();

            var thisMonthPayments = payments.Where(p => p.PaidAt.Date >= thisMonthStart && p.PaidAt.Date < thisMonthEnd).ToList();
            var lastMonthPayments = payments.Where(p => p.PaidAt.Date >= lastMonthStart && p.PaidAt.Date < lastMonthEnd).ToList();

            // Top items
            var orderItems = await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order!.Status == "paid")
                .ToListAsync();

            var topItemsGrouped = orderItems
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new
                {
                    Name = g.First().MenuItem?.Name ?? "Unknown",
                    Qty = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.Qty)
                .Take(5)
                .ToList();

            // Daily chart
            var dailyChartList = new List<object>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayPayments = payments.Where(p => p.PaidAt.Date == date).ToList();
                dailyChartList.Add(new
                {
                    Name = date.ToString("dd/MM"),
                    Revenue = dayPayments.Sum(p => p.TotalAmount),
                    Bills = dayPayments.Count
                });
            }

            return Ok(new
            {
                todayRevenue = todayPayments.Sum(p => p.TotalAmount),
                yesterdayRevenue = yesterdayPayments.Sum(p => p.TotalAmount),
                todayBills = todayPayments.Count,
                yesterdayBills = yesterdayPayments.Count,

                thisWeekRevenue = thisWeekPayments.Sum(p => p.TotalAmount),
                lastWeekRevenue = lastWeekPayments.Sum(p => p.TotalAmount),
                thisWeekBills = thisWeekPayments.Count,
                lastWeekBills = lastWeekPayments.Count,

                thisMonthRevenue = thisMonthPayments.Sum(p => p.TotalAmount),
                lastMonthRevenue = lastMonthPayments.Sum(p => p.TotalAmount),
                thisMonthBills = thisMonthPayments.Count,
                lastMonthBills = lastMonthPayments.Count,

                topItems = topItemsGrouped,
                dailyChart = dailyChartList
            });
        }
    }
}
