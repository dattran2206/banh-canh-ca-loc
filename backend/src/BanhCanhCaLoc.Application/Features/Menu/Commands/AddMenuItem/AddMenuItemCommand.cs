using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.AddMenuItem
{
    public record AddMenuItemCommand(string Name, int CategoryId, decimal Price, string? Description, bool IsAvailable) : ICommand<Result<MenuItem>>;

    public class AddMenuItemCommandValidator : AbstractValidator<AddMenuItemCommand>
    {
        public AddMenuItemCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên món ăn không được để trống")
                               .MaximumLength(150).WithMessage("Tên món ăn tối đa 150 ký tự");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Danh mục không hợp lệ");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá món ăn không hợp lệ");
        }
    }

    public class AddMenuItemCommandHandler : ICommandHandler<AddMenuItemCommand, Result<MenuItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddMenuItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MenuItem>> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
        {
            var item = new MenuItem
            {
                Name = request.Name,
                CategoryId = request.CategoryId,
                Price = request.Price,
                Description = request.Description ?? string.Empty,
                IsAvailable = request.IsAvailable
            };

            await _unitOfWork.MenuItems.AddAsync(item, cancellationToken);

            return item;
        }
    }
}
