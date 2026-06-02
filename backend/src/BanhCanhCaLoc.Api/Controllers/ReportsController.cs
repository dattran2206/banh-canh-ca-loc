using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Application.Features.Reports.Queries.GetDashboardStats;
using BanhCanhCaLoc.Application.Features.Reports.Queries.GetActivityLogs;
using BanhCanhCaLoc.Application.Features.Reports.Commands.AddActivityLog;
using BanhCanhCaLoc.Application.Features.Reports.Queries.GetRevenueChart;
using BanhCanhCaLoc.Application.Features.Reports.Queries.GetReportSummary;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : ApiController
    {
        public ReportsController(ISender sender) : base(sender)
        {
        }

        public class CreateLogDto
        {
            public string Action { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await Sender.Send(new GetDashboardStatsQuery());
            return HandleResult(result);
        }

        [HttpGet("activity-logs")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetActivityLogs()
        {
            var result = await Sender.Send(new GetActivityLogsQuery());

            if (result.IsSuccess)
            {
                return Ok(result.Value.Select(l => new
                {
                    l.Id,
                    l.UserId,
                    UserFullName = l.User != null ? l.User.FullName : "Hệ thống",
                    l.Action,
                    l.Detail,
                    l.Timestamp
                }));
            }

            return HandleResult(result);
        }

        [HttpPost("activity-logs")]
        public async Task<IActionResult> AddActivityLog([FromBody] CreateLogDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new AddActivityLogCommand(dto.Action, dto.Detail, userId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }

            return HandleResult(result);
        }

        [HttpGet("revenue-chart")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetRevenueChartData()
        {
            var result = await Sender.Send(new GetRevenueChartQuery());
            return HandleResult(result);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetReportSummary()
        {
            var result = await Sender.Send(new GetReportSummaryQuery());
            return HandleResult(result);
        }
    }
}
