using FluentValidation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Orders.Commands.CreateOrder
{
    public record CreateOrderItemInput(int MenuItemId, int Quantity, string? Note);

    public record CreateOrderCommand(int TableId, Guid? ShiftId, Guid? UserId, List<CreateOrderItemInput> Items) : ICommand<Result<Order>>;

    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.TableId).GreaterThan(0).WithMessage("Bàn ăn không hợp lệ");
            RuleFor(x => x.Items).NotEmpty().WithMessage("Đơn hàng phải chứa ít nhất 1 món");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.MenuItemId).GreaterThan(0).WithMessage("Món ăn không hợp lệ");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
            });
        }
    }

    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Order>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Order>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync(cancellationToken);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TableId = request.TableId,
                OrderNumber = orderNumber,
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow,
                ShiftId = request.ShiftId
            };

            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Note = item.Note
                };
                order.Items.Add(orderItem);
            }

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "create_order",
                Detail = $"Tạo order {orderNumber} cho Bàn {request.TableId}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return order;
        }
    }
}
