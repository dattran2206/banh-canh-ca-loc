using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.UpdateArea
{
    public record UpdateAreaCommand(int Id, string Name) : ICommand<Result<Area>>;

    public class UpdateAreaCommandValidator : AbstractValidator<UpdateAreaCommand>
    {
        public UpdateAreaCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id khu vực không hợp lệ");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên khu vực không được để trống")
                               .MaximumLength(100).WithMessage("Tên khu vực tối đa 100 ký tự");
        }
    }

    public class UpdateAreaCommandHandler : ICommandHandler<UpdateAreaCommand, Result<Area>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAreaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Area>> Handle(UpdateAreaCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Repository<Area, int>().GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<Area>(new Error("Area.NotFound", "Không tìm thấy khu vực"));
            }

            existing.Name = request.Name;

            _unitOfWork.Repository<Area, int>().Update(existing);

            return existing;
        }
    }
}
