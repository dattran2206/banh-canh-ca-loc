using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public record UpdateOrderStatusResponse(Order Order, List<string> Warnings);

    public record UpdateOrderStatusCommand(Guid OrderId, Guid? UserId, string Status) : ICommand<Result<UpdateOrderStatusResponse>>;

    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId không được rỗng");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Trạng thái không được để trống");
        }
    }

    public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, Result<UpdateOrderStatusResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateOrderStatusResponse>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<UpdateOrderStatusResponse>(new Error("Order.NotFound", "Không tìm thấy đơn hàng"));
            }

            var oldStatus = order.Status;
            order.Status = request.Status;

            var warnings = new List<string>();

            // Auto-deduct inventory if transitioning to "ready"
            if (request.Status == "ready" && oldStatus != "ready")
            {
                warnings = await AutoDeductInventory(order.Id, cancellationToken);
            }

            _unitOfWork.Orders.Update(order);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "update_order_status",
                Detail = $"Cập nhật order {order.OrderNumber} sang {request.Status}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return new UpdateOrderStatusResponse(order, warnings);
        }

        private async Task<List<string>> AutoDeductInventory(Guid orderId, CancellationToken cancellationToken)
        {
            var warnings = new List<string>();
            var orderItems = await _unitOfWork.Repository<OrderItem, Guid>()
                .GetAsync(oi => oi.OrderId == orderId, cancellationToken);

            foreach (var item in orderItems)
            {
                var recipes = await _unitOfWork.Repository<RecipeItem, int>()
                    .GetAsync(r => r.MenuItemId == item.MenuItemId, cancellationToken);

                foreach (var recipe in recipes)
                {
                    double deduct = (recipe.Quantity / recipe.YieldPercent) * item.Quantity;

                    var ingredient = await _unitOfWork.Ingredients.GetByIdAsync(recipe.IngredientId, cancellationToken);
                    if (ingredient != null)
                    {
                        ingredient.CurrentStock = Math.Max(0, ingredient.CurrentStock - deduct);
                        _unitOfWork.Ingredients.Update(ingredient);

                        if (ingredient.CurrentStock <= ingredient.MinThreshold)
                        {
                            warnings.Add(ingredient.Name);
                        }
                    }
                }
            }

            return warnings;
        }
    }
}
