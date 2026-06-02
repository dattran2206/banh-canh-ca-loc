using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.DeleteArea
{
    public record DeleteAreaCommand(int Id) : ICommand<Result>;

    public class DeleteAreaCommandHandler : ICommandHandler<DeleteAreaCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAreaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteAreaCommand request, CancellationToken cancellationToken)
        {
            var area = await _unitOfWork.Repository<Area, int>().GetByIdAsync(request.Id, cancellationToken);
            if (area == null)
            {
                return Result.Failure(new Error("Area.NotFound", "Không tìm thấy khu vực"));
            }

            var hasTables = await _unitOfWork.Tables
                .FirstOrDefaultAsync(t => t.AreaId == request.Id, cancellationToken);

            if (hasTables != null)
            {
                return Result.Failure(new Error("Area.HasTables", "Khu vực đang chứa bàn ăn. Vui lòng di dời hoặc xóa bàn ăn trước."));
            }

            _unitOfWork.Repository<Area, int>().Delete(area);

            return Result.Success();
        }
    }
}
