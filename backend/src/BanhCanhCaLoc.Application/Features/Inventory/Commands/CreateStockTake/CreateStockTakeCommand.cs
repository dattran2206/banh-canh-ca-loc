using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateStockTake
{
    public record CreateStockTakeCommand(int IngredientId, double ActualQty, string? Note, Guid? UserId) : ICommand<Result<StockTake>>;

    public class CreateStockTakeCommandValidator : AbstractValidator<CreateStockTakeCommand>
    {
        public CreateStockTakeCommandValidator()
        {
            RuleFor(x => x.IngredientId).GreaterThan(0).WithMessage("Id nguyên liệu không hợp lệ");
            RuleFor(x => x.ActualQty).GreaterThanOrEqualTo(0).WithMessage("Số lượng kiểm kê thực tế phải lớn hơn hoặc bằng 0");
        }
    }

    public class CreateStockTakeCommandHandler : ICommandHandler<CreateStockTakeCommand, Result<StockTake>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockTakeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<StockTake>> Handle(CreateStockTakeCommand request, CancellationToken cancellationToken)
        {
            var ingredient = await _unitOfWork.Ingredients.GetByIdAsync(request.IngredientId, cancellationToken);
            if (ingredient == null)
            {
                return Result.Failure<StockTake>(new Error("Ingredient.NotFound", "Nguyên liệu không tồn tại"));
            }

            var take = new StockTake
            {
                Id = Guid.NewGuid(),
                IngredientId = request.IngredientId,
                SystemQty = ingredient.CurrentStock,
                ActualQty = request.ActualQty,
                Difference = request.ActualQty - ingredient.CurrentStock,
                CreatedAt = DateTime.UtcNow,
                Note = request.Note
            };

            await _unitOfWork.Repository<StockTake, Guid>().AddAsync(take, cancellationToken);

            ingredient.CurrentStock = request.ActualQty;
            _unitOfWork.Ingredients.Update(ingredient);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "stock_take",
                Detail = $"Kiểm kê {ingredient.Name}. Thực tế: {request.ActualQty}, Lệch: {take.Difference}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return take;
        }
    }
}
