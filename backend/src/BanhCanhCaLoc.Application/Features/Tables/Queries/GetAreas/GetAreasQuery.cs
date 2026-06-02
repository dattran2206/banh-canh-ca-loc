using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Queries.GetAreas
{
    public record GetAreasQuery() : IQuery<Result<IReadOnlyList<Area>>>;

    public class GetAreasQueryHandler : IQueryHandler<GetAreasQuery, Result<IReadOnlyList<Area>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAreasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Area>>> Handle(GetAreasQuery request, CancellationToken cancellationToken)
        {
            var areas = await _unitOfWork.Repository<Area, int>().GetAllAsync(cancellationToken);
            return Result.Success(areas);
        }
    }
}
