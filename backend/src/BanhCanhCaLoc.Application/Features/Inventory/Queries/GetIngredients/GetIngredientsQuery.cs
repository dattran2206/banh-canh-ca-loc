using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Queries.GetIngredients
{
    public record GetIngredientsQuery() : IQuery<Result<IReadOnlyList<Ingredient>>>;

    public class GetIngredientsQueryHandler : IQueryHandler<GetIngredientsQuery, Result<IReadOnlyList<Ingredient>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetIngredientsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Ingredient>>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
        {
            var ingredients = await _unitOfWork.Ingredients.GetAllAsync(cancellationToken);
            return Result.Success(ingredients);
        }
    }
}
