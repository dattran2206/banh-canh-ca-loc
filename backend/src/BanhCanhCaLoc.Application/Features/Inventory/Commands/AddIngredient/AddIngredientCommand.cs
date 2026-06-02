using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.AddIngredient
{
    public record AddIngredientCommand(string Name, string Unit, double MinThreshold, double CurrentStock) : ICommand<Result<Ingredient>>;

    public class AddIngredientCommandValidator : AbstractValidator<AddIngredientCommand>
    {
        public AddIngredientCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nguyên liệu không được để trống")
                               .MaximumLength(150).WithMessage("Tên nguyên liệu tối đa 150 ký tự");
            RuleFor(x => x.Unit).NotEmpty().WithMessage("Đơn vị tính không được để trống")
                               .MaximumLength(50).WithMessage("Đơn vị tính tối đa 50 ký tự");
            RuleFor(x => x.MinThreshold).GreaterThanOrEqualTo(0).WithMessage("Ngưỡng cảnh báo tối thiểu phải lớn hơn hoặc bằng 0");
            RuleFor(x => x.CurrentStock).GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho hiện tại phải lớn hơn hoặc bằng 0");
        }
    }

    public class AddIngredientCommandHandler : ICommandHandler<AddIngredientCommand, Result<Ingredient>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddIngredientCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Ingredient>> Handle(AddIngredientCommand request, CancellationToken cancellationToken)
        {
            var ingredient = new Ingredient
            {
                Name = request.Name,
                Unit = request.Unit,
                MinThreshold = request.MinThreshold,
                CurrentStock = request.CurrentStock
            };

            await _unitOfWork.Ingredients.AddAsync(ingredient, cancellationToken);

            return ingredient;
        }
    }
}
