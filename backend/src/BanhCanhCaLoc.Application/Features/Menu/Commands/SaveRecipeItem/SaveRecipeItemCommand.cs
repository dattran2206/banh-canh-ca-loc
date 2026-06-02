using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.SaveRecipeItem
{
    public record SaveRecipeItemCommand(int MenuItemId, int IngredientId, double Quantity, double YieldPercent) : ICommand<Result<RecipeItem>>;

    public class SaveRecipeItemCommandValidator : AbstractValidator<SaveRecipeItemCommand>
    {
        public SaveRecipeItemCommandValidator()
        {
            RuleFor(x => x.MenuItemId).GreaterThan(0).WithMessage("Id món ăn không hợp lệ");
            RuleFor(x => x.IngredientId).GreaterThan(0).WithMessage("Id nguyên liệu không hợp lệ");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
            RuleFor(x => x.YieldPercent).InclusiveBetween(1, 100).WithMessage("Tỉ lệ hao hụt phải từ 1 đến 100");
        }
    }

    public class SaveRecipeItemCommandHandler : ICommandHandler<SaveRecipeItemCommand, Result<RecipeItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveRecipeItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RecipeItem>> Handle(SaveRecipeItemCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Repository<RecipeItem, int>()
                .FirstOrDefaultAsync(r => r.MenuItemId == request.MenuItemId && r.IngredientId == request.IngredientId, cancellationToken);

            if (existing != null)
            {
                existing.Quantity = request.Quantity;
                existing.YieldPercent = request.YieldPercent;
                _unitOfWork.Repository<RecipeItem, int>().Update(existing);
                return existing;
            }
            else
            {
                var recipe = new RecipeItem
                {
                    MenuItemId = request.MenuItemId,
                    IngredientId = request.IngredientId,
                    Quantity = request.Quantity,
                    YieldPercent = request.YieldPercent
                };
                await _unitOfWork.Repository<RecipeItem, int>().AddAsync(recipe, cancellationToken);
                return recipe;
            }
        }
    }
}
