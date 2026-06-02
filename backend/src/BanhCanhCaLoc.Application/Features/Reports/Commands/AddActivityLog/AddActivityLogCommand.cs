using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Reports.Commands.AddActivityLog
{
    public record AddActivityLogCommand(string Action, string Detail, Guid? UserId) : ICommand<Result>;

    public class AddActivityLogCommandValidator : AbstractValidator<AddActivityLogCommand>
    {
        public AddActivityLogCommandValidator()
        {
            RuleFor(x => x.Action).NotEmpty().WithMessage("Action không được để trống")
                                 .MaximumLength(100).WithMessage("Action tối đa 100 ký tự");
            RuleFor(x => x.Detail).NotEmpty().WithMessage("Chi tiết không được để trống");
        }
    }

    public class AddActivityLogCommandHandler : ICommandHandler<AddActivityLogCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddActivityLogCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddActivityLogCommand request, CancellationToken cancellationToken)
        {
            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = request.Action,
                Detail = request.Detail,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(log, cancellationToken);

            return Result.Success();
        }
    }
}
