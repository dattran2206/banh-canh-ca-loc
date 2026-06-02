using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Staff.Commands.ToggleStaffActive
{
    public record ToggleStaffActiveCommand(Guid TargetUserId, Guid? CurrentUserId) : ICommand<Result<User>>;

    public class ToggleStaffActiveCommandHandler : ICommandHandler<ToggleStaffActiveCommand, Result<User>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ToggleStaffActiveCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<User>> Handle(ToggleStaffActiveCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserId.HasValue && request.CurrentUserId.Value == request.TargetUserId)
            {
                return Result.Failure<User>(new Error("Staff.SelfDeactivation", "Không thể tự khóa tài khoản của chính mình"));
            }

            var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<User>(new Error("Staff.NotFound", "Không tìm thấy tài khoản nhân viên"));
            }

            user.IsActive = !user.IsActive;
            _unitOfWork.Users.Update(user);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.CurrentUserId,
                Action = "toggle_active_user",
                Detail = $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản nhân viên {user.FullName}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return user;
        }
    }
}
