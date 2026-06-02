using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.UpdateMenuItem
{
    public record UpdateMenuItemCommand(int Id, string Name, int CategoryId, decimal Price, string? Description, bool IsAvailable) : ICommand<Result<MenuItem>>;

    public class UpdateMenuItemCommandValidator : AbstractValidator<UpdateMenuItemCommand>
    {
        public UpdateMenuItemCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id món ăn không hợp lệ");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên món ăn không được để trống")
                               .MaximumLength(150).WithMessage("Tên món ăn tối đa 150 ký tự");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Danh mục không hợp lệ");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá món ăn không hợp lệ");
        }
    }

    public class UpdateMenuItemCommandHandler : ICommandHandler<UpdateMenuItemCommand, Result<MenuItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMenuItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MenuItem>> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.MenuItems.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<MenuItem>(new Error("MenuItem.NotFound", "Không tìm thấy món ăn"));
            }

            existing.Name = request.Name;
            existing.CategoryId = request.CategoryId;
            existing.Price = request.Price;
            existing.Description = request.Description ?? string.Empty;
            existing.IsAvailable = request.IsAvailable;

            _unitOfWork.MenuItems.Update(existing);

            return existing;
        }
    }
}
