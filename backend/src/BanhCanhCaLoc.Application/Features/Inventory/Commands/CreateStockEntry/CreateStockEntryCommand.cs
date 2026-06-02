using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateStockEntry
{
    public record CreateStockEntryCommand(int IngredientId, double Quantity, decimal UnitPrice, string? Note, Guid? UserId) : ICommand<Result<StockEntry>>;

    public class CreateStockEntryCommandValidator : AbstractValidator<CreateStockEntryCommand>
    {
        public CreateStockEntryCommandValidator()
        {
            RuleFor(x => x.IngredientId).GreaterThan(0).WithMessage("Id nguyên liệu không hợp lệ");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng nhập kho phải lớn hơn 0");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Đơn giá nhập kho không hợp lệ");
        }
    }

    public class CreateStockEntryCommandHandler : ICommandHandler<CreateStockEntryCommand, Result<StockEntry>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockEntryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<StockEntry>> Handle(CreateStockEntryCommand request, CancellationToken cancellationToken)
        {
            var ingredient = await _unitOfWork.Ingredients.GetByIdAsync(request.IngredientId, cancellationToken);
            if (ingredient == null)
            {
                return Result.Failure<StockEntry>(new Error("Ingredient.NotFound", "Nguyên liệu không tồn tại"));
            }

            var entry = new StockEntry
            {
                Id = Guid.NewGuid(),
                IngredientId = request.IngredientId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                Note = request.Note
            };

            await _unitOfWork.Repository<StockEntry, Guid>().AddAsync(entry, cancellationToken);

            ingredient.CurrentStock += request.Quantity;
            _unitOfWork.Ingredients.Update(ingredient);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "add_stock",
                Detail = $"Nhập kho {request.Quantity} {ingredient.Unit} {ingredient.Name}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return entry;
        }
    }
}
