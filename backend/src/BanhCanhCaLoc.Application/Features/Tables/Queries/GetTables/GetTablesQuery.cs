using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Queries.GetTables
{
    public record GetTablesQuery() : IQuery<Result<IReadOnlyList<Table>>>;

    public class GetTablesQueryHandler : IQueryHandler<GetTablesQuery, Result<IReadOnlyList<Table>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTablesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Table>>> Handle(GetTablesQuery request, CancellationToken cancellationToken)
        {
            var tables = await _unitOfWork.Tables.GetTablesWithAreaAsync(cancellationToken);
            return Result.Success(tables);
        }
    }
}
