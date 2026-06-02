using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Auth.Commands.StartShift
{
    public record StartShiftCommand(Guid UserId) : ICommand<Result<Shift>>;

    public class StartShiftCommandHandler : ICommandHandler<StartShiftCommand, Result<Shift>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public StartShiftCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Shift>> Handle(StartShiftCommand request, CancellationToken cancellationToken)
        {
            // Check if there is an active shift for this user
            var activeShift = await _unitOfWork.Repository<Shift, Guid>()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.EndTime == null, cancellationToken);

            if (activeShift != null)
            {
                return Result.Failure<Shift>(new Error("Shift.AlreadyActive", "Bạn đang có ca làm việc chưa kết thúc"));
            }

            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                TotalRevenue = 0,
                TotalBills = 0
            };

            await _unitOfWork.Repository<Shift, Guid>().AddAsync(shift, cancellationToken);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "start_shift",
                Detail = "Bắt đầu ca làm việc",
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return shift;
        }
    }
}
