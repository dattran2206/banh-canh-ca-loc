using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    public class OrdersController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrdersController(BanhCanhCaLocDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public class CreateOrderItemDto
        {
            public int MenuItemId { get; set; }
            public int Quantity { get; set; }
            public string? Note { get; set; }
        }

        public class CreateOrderDto
        {
            public int TableId { get; set; }
            public Guid? ShiftId { get; set; }
            public List<CreateOrderItemDto> Items { get; set; } = new();
        }

        public class UpdateStatusDto
        {
            public string Status { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? status)
        {
            var query = _context.Orders.Include(o => o.Table).AsQueryable();

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

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderList = new List<object>();
            foreach (var o in orders)
            {
                var items = await _context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .Where(oi => oi.OrderId == o.Id)
                    .ToListAsync();

                orderList.Add(new
                {
                    o.Id,
                    o.TableId,
                    Table = new { o.Table?.Id, o.Table?.Number, o.Table?.Area, o.Table?.Capacity },
                    o.OrderNumber,
                    o.Status,
                    o.CreatedAt,
                    o.ShiftId,
                    Items = items.Select(i => new
                    {
                        i.Id,
                        i.OrderId,
                        i.MenuItemId,
                        MenuItem = new { i.MenuItem?.Id, i.MenuItem?.Name, i.MenuItem?.Price },
                        i.Quantity,
                        i.Note
                    })
                });
            }

            return Ok(orderList);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var activeOrders = await _context.Orders
                .Include(o => o.Table)
                .Where(o => o.Status != "paid")
                .ToListAsync();

            var orderList = new List<object>();
            foreach (var o in activeOrders)
            {
                var items = await _context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .Where(oi => oi.OrderId == o.Id)
                    .ToListAsync();

                orderList.Add(new
                {
                    o.Id,
                    o.TableId,
                    Table = new { o.Table?.Id, o.Table?.Number, o.Table?.Area, o.Table?.Capacity },
                    o.OrderNumber,
                    o.Status,
                    o.CreatedAt,
                    o.ShiftId,
                    Items = items.Select(i => new
                    {
                        i.Id,
                        i.OrderId,
                        i.MenuItemId,
                        MenuItem = new { i.MenuItem?.Id, i.MenuItem?.Name, i.MenuItem?.Price },
                        i.Quantity,
                        i.Note
                    })
                });
            }

            return Ok(orderList);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var countToday = await _context.Orders
                .CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date);
            var orderNumber = $"OD-{(countToday + 1):D3}";

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TableId = dto.TableId,
                OrderNumber = orderNumber,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                ShiftId = dto.ShiftId
            };

            _context.Orders.Add(order);

            foreach (var item in dto.Items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Note = item.Note
                });
            }

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "create_order",
                Detail = $"Tạo order {orderNumber} cho Bàn {dto.TableId}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // BroadCast Realtime via SignalR
            await NotifyOrderUpdated(order.Id);

            return Ok(order);
        }

        [Authorize]
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AppendOrderItems(Guid id, [FromBody] List<CreateOrderItemDto> items)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == "paid")
            {
                return BadRequest(new { message = "Không thể thêm món vào hóa đơn đã thanh toán" });
            }

            foreach (var item in items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Note = item.Note
                });
            }

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "add_order_items",
                Detail = $"Thêm món vào order {order.OrderNumber} bàn {order.TableId}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // BroadCast Realtime via SignalR
            await NotifyOrderUpdated(order.Id);

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            order.Status = dto.Status;

            var warnings = new List<string>();

            // Auto-deduct inventory if transitioning to "ready"
            if (dto.Status == "ready" && oldStatus != "ready")
            {
                warnings = await AutoDeductInventory(order.Id);
            }

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "update_order_status",
                Detail = $"Cập nhật order {order.OrderNumber} sang {dto.Status}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // BroadCast Realtime
            await NotifyOrderUpdated(order.Id);

            return Ok(new { order, warnings });
        }

        private async Task<List<string>> AutoDeductInventory(Guid orderId)
        {
            var warnings = new List<string>();
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var recipes = await _context.RecipeItems
                    .Where(r => r.MenuItemId == item.MenuItemId)
                    .ToListAsync();

                foreach (var recipe in recipes)
                {
                    double deduct = (recipe.Quantity / recipe.YieldPercent) * item.Quantity;

                    var ingredient = await _context.Ingredients.FindAsync(recipe.IngredientId);
                    if (ingredient != null)
                    {
                        ingredient.CurrentStock = Math.Max(0, ingredient.CurrentStock - deduct);
                        if (ingredient.CurrentStock <= ingredient.MinThreshold)
                        {
                            warnings.Add(ingredient.Name);
                        }
                    }
                }
            }

            return warnings;
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
