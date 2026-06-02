using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BanhCanhCaLoc.Api.Data;
using BanhCanhCaLoc.Api.Hubs;
using BanhCanhCaLoc.Api.Models;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public PaymentsController(BanhCanhCaLocDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public class CreatePaymentDto
        {
            public Guid OrderId { get; set; }
            public decimal TotalAmount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound();

            if (order.Status == "paid")
            {
                return BadRequest(new { message = "Hóa đơn này đã được thanh toán trước đó" });
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = dto.OrderId,
                TotalAmount = dto.TotalAmount,
                PaidAt = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod
            };

            _context.Payments.Add(payment);

            order.Status = "paid";

            // Update Shift totals if this order is linked to a shift
            if (order.ShiftId.HasValue)
            {
                var shift = await _context.Shifts.FindAsync(order.ShiftId.Value);
                if (shift != null)
                {
                    shift.TotalRevenue += dto.TotalAmount;
                    shift.TotalBills += 1;
                }
            }

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "payment",
                Detail = $"Thanh toán order {order.OrderNumber} bàn {order.TableId}, {dto.TotalAmount.ToString("N0")}đ bằng {dto.PaymentMethod}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // BroadCast Realtime via SignalR
            await NotifyOrderUpdated(order.Id);

            return Ok(payment);
        }

        private async Task NotifyOrderUpdated(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                var items = await _context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .Where(oi => oi.OrderId == order.Id)
                    .ToListAsync();

                var orderData = new
                {
                    order.Id,
                    order.TableId,
                    Table = new { order.Table?.Id, order.Table?.Number, order.Table?.Area, order.Table?.Capacity },
                    order.OrderNumber,
                    order.Status,
                    order.CreatedAt,
                    order.ShiftId,
                    Items = items.Select(i => new
                    {
                        i.Id,
                        i.OrderId,
                        i.MenuItemId,
                        MenuItem = new { i.MenuItem?.Id, i.MenuItem?.Name, i.MenuItem?.Price },
                        i.Quantity,
                        i.Note
                    })
                };

                await _hubContext.Clients.All.SendAsync("OrderUpdated", orderData);
            }
        }
    }
}
