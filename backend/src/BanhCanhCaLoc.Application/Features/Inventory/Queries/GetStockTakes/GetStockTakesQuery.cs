using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Queries.GetStockTakes
{
    public record GetStockTakesQuery() : IQuery<Result<IReadOnlyList<StockTake>>>;

    public class GetStockTakesQueryHandler : IQueryHandler<GetStockTakesQuery, Result<IReadOnlyList<StockTake>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStockTakesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<StockTake>>> Handle(GetStockTakesQuery request, CancellationToken cancellationToken)
        {
            var takes = await _unitOfWork.Repository<StockTake, Guid>().GetAllAsync(cancellationToken);
            var ingredients = await _unitOfWork.Ingredients.GetAllAsync(cancellationToken);
            var ingredientMap = ingredients.ToDictionary(i => i.Id);

            foreach (var take in takes)
            {
                if (ingredientMap.TryGetValue(take.IngredientId, out var ingredient))
                {
                    take.Ingredient = ingredient;
                }
            }

            var orderedTakes = takes.OrderByDescending(s => s.CreatedAt).ToList();
            return Result.Success<IReadOnlyList<StockTake>>(orderedTakes);
        }
    }
}
