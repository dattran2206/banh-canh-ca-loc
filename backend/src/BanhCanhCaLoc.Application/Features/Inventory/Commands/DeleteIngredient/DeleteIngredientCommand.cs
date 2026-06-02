using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Commands.DeleteIngredient
{
    public record DeleteIngredientCommand(int Id) : ICommand<Result>;

    public class DeleteIngredientCommandHandler : ICommandHandler<DeleteIngredientCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteIngredientCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
        {
            var ingredient = await _unitOfWork.Ingredients.GetByIdAsync(request.Id, cancellationToken);
            if (ingredient == null)
            {
                return Result.Failure(new Error("Ingredient.NotFound", "Không tìm thấy nguyên liệu"));
            }

            _unitOfWork.Ingredients.Delete(ingredient);

            return Result.Success();
        }
    }
}
