using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.AddArea
{
    public record AddAreaCommand(string Name) : ICommand<Result<Area>>;

    public class AddAreaCommandValidator : AbstractValidator<AddAreaCommand>
    {
        public AddAreaCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên khu vực không được để trống")
                               .MaximumLength(100).WithMessage("Tên khu vực tối đa 100 ký tự");
        }
    }

    public class AddAreaCommandHandler : ICommandHandler<AddAreaCommand, Result<Area>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddAreaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Area>> Handle(AddAreaCommand request, CancellationToken cancellationToken)
        {
            var area = new Area
            {
                Name = request.Name
            };

            await _unitOfWork.Repository<Area, int>().AddAsync(area, cancellationToken);

            return area;
        }
    }
}
