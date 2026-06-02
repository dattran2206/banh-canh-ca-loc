using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Orders.Queries.GetOrders
{
    public record GetOrdersQuery(string? Status) : IQuery<Result<IReadOnlyList<Order>>>;

    public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, Result<IReadOnlyList<Order>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Order>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.Orders.GetOrdersWithDetailsAsync(request.Status, cancellationToken);
            return Result.Success(orders);
        }
    }
}
