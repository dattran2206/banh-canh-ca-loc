using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Staff.Commands.DeleteStaff
{
    public record DeleteStaffCommand(Guid TargetUserId, Guid? CurrentUserId) : ICommand<Result>;

    public class DeleteStaffCommandHandler : ICommandHandler<DeleteStaffCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStaffCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserId.HasValue && request.CurrentUserId.Value == request.TargetUserId)
            {
                return Result.Failure(new Error("Staff.SelfDeletion", "Không thể tự xóa tài khoản của chính mình"));
            }

            var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure(new Error("Staff.NotFound", "Không tìm thấy tài khoản nhân viên"));
            }

            _unitOfWork.Users.Delete(user);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.CurrentUserId,
                Action = "delete_user",
                Detail = $"Xóa tài khoản nhân viên {user.FullName}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return Result.Success();
        }
    }
}
