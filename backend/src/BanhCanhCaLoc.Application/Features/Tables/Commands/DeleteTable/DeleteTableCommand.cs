using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.DeleteTable
{
    public record DeleteTableCommand(int Id) : ICommand<Result>;

    public class DeleteTableCommandHandler : ICommandHandler<DeleteTableCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTableCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
        {
            var table = await _unitOfWork.Tables.GetByIdAsync(request.Id, cancellationToken);
            if (table == null)
            {
                return Result.Failure(new Error("Table.NotFound", "Không tìm thấy bàn ăn"));
            }

            // Check if there are active orders for this table
            var activeOrders = await _unitOfWork.Orders
                .FirstOrDefaultAsync(o => o.TableId == request.Id && o.Status != "paid", cancellationToken);

            if (activeOrders != null)
            {
                return Result.Failure(new Error("Table.HasActiveOrders", "Bàn ăn đang có đơn hàng chưa hoàn tất thanh toán"));
            }

            _unitOfWork.Tables.Delete(table);

            return Result.Success();
        }
    }
}
