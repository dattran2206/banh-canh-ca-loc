using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteRecipeItem
{
    public record DeleteRecipeItemCommand(int MenuItemId, int IngredientId) : ICommand<Result>;

    public class DeleteRecipeItemCommandHandler : ICommandHandler<DeleteRecipeItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRecipeItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteRecipeItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _unitOfWork.Repository<RecipeItem, int>()
                .FirstOrDefaultAsync(r => r.MenuItemId == request.MenuItemId && r.IngredientId == request.IngredientId, cancellationToken);

            if (item == null)
            {
                return Result.Failure(new Error("RecipeItem.NotFound", "Không tìm thấy nguyên liệu trong công thức"));
            }

            _unitOfWork.Repository<RecipeItem, int>().Delete(item);

            return Result.Success();
        }
    }
}
