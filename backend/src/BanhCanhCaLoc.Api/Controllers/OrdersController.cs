using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BanhCanhCaLoc.Api.Hubs;
using BanhCanhCaLoc.Contracts.Orders;
using BanhCanhCaLoc.Application.Features.Orders.Queries.GetOrders;
using BanhCanhCaLoc.Application.Features.Orders.Queries.GetOrderById;
using BanhCanhCaLoc.Application.Features.Orders.Commands.CreateOrder;
using BanhCanhCaLoc.Application.Features.Orders.Commands.AppendOrderItems;
using BanhCanhCaLoc.Application.Features.Orders.Commands.UpdateOrderStatus;
using BanhCanhCaLoc.Application.Features.Orders.Commands.UpdateOrderItems;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ApiController
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrdersController(ISender sender, IHubContext<OrderHub> hubContext) : base(sender)
        {
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? status)
        {
            var query = new GetOrdersQuery(status);
            var result = await Sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(MapOrders(result.Value));
            }

            return HandleResult(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var query = new GetOrdersQuery("active");
            var result = await Sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(MapOrders(result.Value));
            }

            return HandleResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var itemsInput = request.Items.Select(i => new CreateOrderItemInput(i.MenuItemId, i.Quantity, i.Note)).ToList();
            var command = new CreateOrderCommand(request.TableId, request.ShiftId, userId, itemsInput);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                await NotifyOrderUpdated(result.Value.Id);
                return Ok(result.Value);
            }

            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AppendOrderItems(Guid id, [FromBody] List<CreateOrderItemRequest> items)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var itemsInput = items.Select(i => new CreateOrderItemInput(i.MenuItemId, i.Quantity, i.Note)).ToList();
            var command = new AppendOrderItemsCommand(id, userId, itemsInput);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                await NotifyOrderUpdated(id);
                return Ok(new { success = true });
            }

            return HandleResult(result);
        }

        [Authorize]
        [HttpPut("{id}/items")]
        public async Task<IActionResult> UpdateOrderItems(Guid id, [FromBody] List<CreateOrderItemRequest> items)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var itemsInput = items.Select(i => new CreateOrderItemInput(i.MenuItemId, i.Quantity, i.Note)).ToList();
            var command = new UpdateOrderItemsCommand(id, userId, itemsInput);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                await NotifyOrderUpdated(id);
                return Ok(new { success = true, warnings = result.Value.Warnings });
            }

            return HandleResult(result);
        }

        [Authorize]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new UpdateOrderStatusCommand(id, userId, request.Status);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                await NotifyOrderUpdated(id);
                return Ok(new { order = result.Value.Order, warnings = result.Value.Warnings });
            }

            return HandleResult(result);
        }

        private async Task NotifyOrderUpdated(Guid orderId)
        {
            var query = new GetOrderByIdQuery(orderId);
            var result = await Sender.Send(query);

            if (result.IsSuccess)
            {
                var order = result.Value;
                var orderData = new
                {
                    order.Id,
                    order.TableId,
                    Table = new { order.Table?.Id, order.Table?.Number, order.Table?.Area, order.Table?.Capacity },
                    order.OrderNumber,
                    order.Status,
                    order.CreatedAt,
                    order.ShiftId,
                    Items = order.Items.Select(i => new
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

        private static List<object> MapOrders(IReadOnlyList<Order> orders)
        {
            return orders.Select(o => (object)new
            {
                o.Id,
                o.TableId,
                Table = new { o.Table?.Id, o.Table?.Number, o.Table?.Area, o.Table?.Capacity },
                o.OrderNumber,
                o.Status,
                o.CreatedAt,
                o.ShiftId,
                Items = o.Items.Select(i => new
                {
                    i.Id,
                    i.OrderId,
                    i.MenuItemId,
                    MenuItem = new { i.MenuItem?.Id, i.MenuItem?.Name, i.MenuItem?.Price },
                    i.Quantity,
                    i.Note
                })
            }).ToList();
        }
    }
}
