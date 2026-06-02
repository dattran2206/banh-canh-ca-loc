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
using BanhCanhCaLoc.Application.Features.Orders.Commands.CreateOrder;

namespace BanhCanhCaLoc.Application.Features.Orders.Commands.UpdateOrderItems
{
    public record UpdateOrderItemsResponse(List<string> Warnings);

    public record UpdateOrderItemsCommand(Guid OrderId, Guid? UserId, List<CreateOrderItemInput> Items) : ICommand<Result<UpdateOrderItemsResponse>>;

    public class UpdateOrderItemsCommandValidator : AbstractValidator<UpdateOrderItemsCommand>
    {
        public UpdateOrderItemsCommandValidator()
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

    public class UpdateOrderItemsCommandHandler : ICommandHandler<UpdateOrderItemsCommand, Result<UpdateOrderItemsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderItemsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateOrderItemsResponse>> Handle(UpdateOrderItemsCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<UpdateOrderItemsResponse>(new Error("Order.NotFound", "Không tìm thấy đơn hàng"));
            }

            if (order.Status == "paid")
            {
                return Result.Failure<UpdateOrderItemsResponse>(new Error("Order.AlreadyPaid", "Không thể chỉnh sửa hóa đơn đã thanh toán"));
            }

            var warnings = new List<string>();

            // If the order is already ready (cooked), deduct additional stock for added/increased quantities
            if (order.Status == "ready")
            {
                var oldItems = await _unitOfWork.Repository<OrderItem, Guid>()
                    .GetAsync(oi => oi.OrderId == order.Id, cancellationToken);

                warnings = await DeductAdditionalInventory(oldItems, request.Items, cancellationToken);
            }

            // Remove existing items
            var existingItems = await _unitOfWork.Repository<OrderItem, Guid>()
                .GetAsync(oi => oi.OrderId == order.Id, cancellationToken);

            foreach (var item in existingItems)
            {
                _unitOfWork.Repository<OrderItem, Guid>().Delete(item);
            }

            // Add new items
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
                Action = "update_order_items",
                Detail = $"Chỉnh sửa/Đổi món trong order {order.OrderNumber} bàn {order.TableId}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return new UpdateOrderItemsResponse(warnings);
        }

        private async Task<List<string>> DeductAdditionalInventory(
            IReadOnlyList<OrderItem> oldItems, 
            List<CreateOrderItemInput> newItems, 
            CancellationToken cancellationToken)
        {
            var warnings = new List<string>();

            foreach (var newItem in newItems)
            {
                var oldItem = oldItems.FirstOrDefault(oi => oi.MenuItemId == newItem.MenuItemId);
                int oldQty = oldItem?.Quantity ?? 0;
                int diffQty = newItem.Quantity - oldQty;

                if (diffQty > 0)
                {
                    // Deduct ingredients for the extra diffQty
                    var recipes = await _unitOfWork.Repository<RecipeItem, int>()
                        .GetAsync(r => r.MenuItemId == newItem.MenuItemId, cancellationToken);

                    foreach (var recipe in recipes)
                    {
                        double deduct = (recipe.Quantity / recipe.YieldPercent) * diffQty;

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
            }

            return warnings;
        }
    }
}
