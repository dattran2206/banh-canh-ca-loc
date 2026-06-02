using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Queries.GetStockEntries
{
    public record GetStockEntriesQuery() : IQuery<Result<IReadOnlyList<StockEntry>>>;

    public class GetStockEntriesQueryHandler : IQueryHandler<GetStockEntriesQuery, Result<IReadOnlyList<StockEntry>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStockEntriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<StockEntry>>> Handle(GetStockEntriesQuery request, CancellationToken cancellationToken)
        {
            var entries = await _unitOfWork.Repository<StockEntry, Guid>().GetAllAsync(cancellationToken);
            var ingredients = await _unitOfWork.Ingredients.GetAllAsync(cancellationToken);
            var ingredientMap = ingredients.ToDictionary(i => i.Id);

            foreach (var entry in entries)
            {
                if (ingredientMap.TryGetValue(entry.IngredientId, out var ingredient))
                {
                    entry.Ingredient = ingredient;
                }
            }

            var orderedEntries = entries.OrderByDescending(s => s.CreatedAt).ToList();
            return Result.Success<IReadOnlyList<StockEntry>>(orderedEntries);
        }
    }
}
