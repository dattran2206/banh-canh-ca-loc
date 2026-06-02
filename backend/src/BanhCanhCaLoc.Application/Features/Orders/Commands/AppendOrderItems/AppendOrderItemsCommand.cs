using FluentValidation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Orders.Commands.CreateOrder;

namespace BanhCanhCaLoc.Application.Features.Orders.Commands.AppendOrderItems
{
    public record AppendOrderItemsCommand(Guid OrderId, Guid? UserId, List<CreateOrderItemInput> Items) : ICommand<Result>;

    public class AppendOrderItemsCommandValidator : AbstractValidator<AppendOrderItemsCommand>
    {
        public AppendOrderItemsCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId không được rỗng");
            RuleFor(x => x.Items).NotEmpty().WithMessage("Danh sách món ăn không được rỗng");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.MenuItemId).GreaterThan(0).WithMessage("Món ăn không hợp lệ");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
            });
        }
    }

    public class AppendOrderItemsCommandHandler : ICommandHandler<AppendOrderItemsCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppendOrderItemsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AppendOrderItemsCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure(new Error("Order.NotFound", "Không tìm thấy đơn hàng"));
            }

            if (order.Status == "paid")
            {
                return Result.Failure(new Error("Order.AlreadyPaid", "Không thể thêm món vào hóa đơn đã thanh toán"));
            }

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
                await _unitOfWork.Repository<OrderItem, Guid>().AddAsync(orderItem, cancellationToken);
            }

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "add_order_items",
                Detail = $"Thêm món vào order {order.OrderNumber} bàn {order.TableId}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return Result.Success();
        }
    }
}
