using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.UpdateIngredient
{
    public record UpdateIngredientCommand(int Id, string Name, string Unit, double MinThreshold, double CurrentStock) : ICommand<Result<Ingredient>>;

    public class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
    {
        public UpdateIngredientCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id nguyên liệu không hợp lệ");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nguyên liệu không được để trống")
                               .MaximumLength(150).WithMessage("Tên nguyên liệu tối đa 150 ký tự");
            RuleFor(x => x.Unit).NotEmpty().WithMessage("Đơn vị tính không được để trống")
                               .MaximumLength(50).WithMessage("Đơn vị tính tối đa 50 ký tự");
            RuleFor(x => x.MinThreshold).GreaterThanOrEqualTo(0).WithMessage("Ngưỡng cảnh báo tối thiểu phải lớn hơn hoặc bằng 0");
            RuleFor(x => x.CurrentStock).GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho hiện tại phải lớn hơn hoặc bằng 0");
        }
    }

    public class UpdateIngredientCommandHandler : ICommandHandler<UpdateIngredientCommand, Result<Ingredient>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateIngredientCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Ingredient>> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Ingredients.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<Ingredient>(new Error("Ingredient.NotFound", "Không tìm thấy nguyên liệu"));
            }

            existing.Name = request.Name;
            existing.Unit = request.Unit;
            existing.MinThreshold = request.MinThreshold;
            existing.CurrentStock = request.CurrentStock;

            _unitOfWork.Ingredients.Update(existing);

            return existing;
        }
    }
}
