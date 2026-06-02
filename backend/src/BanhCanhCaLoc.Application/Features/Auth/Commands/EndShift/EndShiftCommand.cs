using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Auth.Commands.EndShift
{
    public record EndShiftCommand(Guid UserId) : ICommand<Result<Shift>>;

    public class EndShiftCommandHandler : ICommandHandler<EndShiftCommand, Result<Shift>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EndShiftCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Shift>> Handle(EndShiftCommand request, CancellationToken cancellationToken)
        {
            var activeShift = await _unitOfWork.Repository<Shift, Guid>()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.EndTime == null, cancellationToken);

            if (activeShift == null)
            {
                return Result.Failure<Shift>(new Error("Shift.NotFound", "Không tìm thấy ca làm việc đang hoạt động"));
            }

            // Calculate revenue and bills for this shift
            var shiftPayments = await _unitOfWork.Repository<Payment, Guid>()
                .GetAsync(p => p.Order != null && p.Order.ShiftId == activeShift.Id, cancellationToken);

            activeShift.EndTime = DateTime.UtcNow;
            activeShift.TotalRevenue = shiftPayments.Sum(p => p.TotalAmount);
            activeShift.TotalBills = shiftPayments.Count;

            _unitOfWork.Repository<Shift, Guid>().Update(activeShift);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "end_shift",
                Detail = "Kết thúc ca làm việc",
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return activeShift;
        }
    }
}
