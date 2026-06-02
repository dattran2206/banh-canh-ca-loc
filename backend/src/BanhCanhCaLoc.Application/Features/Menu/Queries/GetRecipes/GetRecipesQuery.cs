using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Queries.GetRecipes
{
    public record GetRecipesQuery(int MenuItemId) : IQuery<Result<IReadOnlyList<RecipeItem>>>;

    public class GetRecipesQueryHandler : IQueryHandler<GetRecipesQuery, Result<IReadOnlyList<RecipeItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRecipesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<RecipeItem>>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
        {
            var recipes = await _unitOfWork.Repository<RecipeItem, int>()
                .GetAsync(r => r.MenuItemId == request.MenuItemId, cancellationToken);
            return Result.Success(recipes);
        }
    }
}
