using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BanhCanhCaLoc.Api.Hubs;
using BanhCanhCaLoc.Contracts.Orders;
using BanhCanhCaLoc.Application.Features.Orders.Commands.CreatePayment;
using BanhCanhCaLoc.Application.Features.Orders.Queries.GetOrderById;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class PaymentsController : ApiController
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public PaymentsController(ISender sender, IHubContext<OrderHub> hubContext) : base(sender)
        {
            _hubContext = hubContext;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new CreatePaymentCommand(request.OrderId, request.TotalAmount, request.PaymentMethod, userId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                await NotifyOrderUpdated(request.OrderId);
                return Ok(result.Value);
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
    }
}
