using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateWasteRecord
{
    public record CreateWasteRecordCommand(int IngredientId, double Quantity, string Reason, Guid? UserId) : ICommand<Result<WasteRecord>>;

    public class CreateWasteRecordCommandValidator : AbstractValidator<CreateWasteRecordCommand>
    {
        public CreateWasteRecordCommandValidator()
        {
            RuleFor(x => x.IngredientId).GreaterThan(0).WithMessage("Id nguyên liệu không hợp lệ");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng hủy kho phải lớn hơn 0");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("Lý do hủy kho không được để trống")
                                 .MaximumLength(250).WithMessage("Lý do hủy kho tối đa 250 ký tự");
        }
    }

    public class CreateWasteRecordCommandHandler : ICommandHandler<CreateWasteRecordCommand, Result<WasteRecord>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateWasteRecordCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WasteRecord>> Handle(CreateWasteRecordCommand request, CancellationToken cancellationToken)
        {
            var ingredient = await _unitOfWork.Ingredients.GetByIdAsync(request.IngredientId, cancellationToken);
            if (ingredient == null)
            {
                return Result.Failure<WasteRecord>(new Error("Ingredient.NotFound", "Nguyên liệu không tồn tại"));
            }

            if (ingredient.CurrentStock < request.Quantity)
            {
                return Result.Failure<WasteRecord>(new Error("Inventory.InsufficientStock", "Lượng hủy kho vượt quá lượng tồn kho hiện tại"));
            }

            var record = new WasteRecord
            {
                Id = Guid.NewGuid(),
                IngredientId = request.IngredientId,
                Quantity = request.Quantity,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<WasteRecord, Guid>().AddAsync(record, cancellationToken);

            ingredient.CurrentStock = Math.Max(0, ingredient.CurrentStock - request.Quantity);
            _unitOfWork.Ingredients.Update(ingredient);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "waste_stock",
                Detail = $"Hủy kho {request.Quantity} {ingredient.Unit} {ingredient.Name}. Lý do: {request.Reason}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return record;
        }
    }
}
