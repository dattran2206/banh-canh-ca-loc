using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Inventory.Queries.GetWasteRecords
{
    public record GetWasteRecordsQuery() : IQuery<Result<IReadOnlyList<WasteRecord>>>;

    public class GetWasteRecordsQueryHandler : IQueryHandler<GetWasteRecordsQuery, Result<IReadOnlyList<WasteRecord>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWasteRecordsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<WasteRecord>>> Handle(GetWasteRecordsQuery request, CancellationToken cancellationToken)
        {
            var records = await _unitOfWork.Repository<WasteRecord, Guid>().GetAllAsync(cancellationToken);
            var ingredients = await _unitOfWork.Ingredients.GetAllAsync(cancellationToken);
            var ingredientMap = ingredients.ToDictionary(i => i.Id);

            foreach (var record in records)
            {
                if (ingredientMap.TryGetValue(record.IngredientId, out var ingredient))
                {
                    record.Ingredient = ingredient;
                }
            }

            var orderedRecords = records.OrderByDescending(r => r.CreatedAt).ToList();
            return Result.Success<IReadOnlyList<WasteRecord>>(orderedRecords);
        }
    }
}
