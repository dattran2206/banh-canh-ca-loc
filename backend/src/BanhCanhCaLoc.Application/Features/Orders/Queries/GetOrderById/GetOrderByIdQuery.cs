using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<Order>>;

    public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<Order>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Order>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetWithItemsAndTableAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<Order>(new Error("Order.NotFound", "Không tìm thấy đơn hàng"));
            }

            return order;
        }
    }
}
