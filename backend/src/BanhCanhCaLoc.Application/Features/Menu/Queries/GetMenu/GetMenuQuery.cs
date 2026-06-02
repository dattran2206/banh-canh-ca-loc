using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Queries.GetMenu
{
    public record GetMenuQuery() : IQuery<Result<IReadOnlyList<MenuItem>>>;

    public class GetMenuQueryHandler : IQueryHandler<GetMenuQuery, Result<IReadOnlyList<MenuItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMenuQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<MenuItem>>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuItems.GetMenuItemsWithDetailsAsync(cancellationToken);
            return Result.Success(menu);
        }
    }
}
