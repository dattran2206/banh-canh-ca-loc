using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.AddCategory
{
    public record AddCategoryCommand(string Name) : ICommand<Result<Category>>;

    public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public AddCategoryCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên danh mục không được để trống")
                               .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự");
        }
    }

    public class AddCategoryCommandHandler : ICommandHandler<AddCategoryCommand, Result<Category>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Category>> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name
            };

            await _unitOfWork.Repository<Category, int>().AddAsync(category, cancellationToken);

            return category;
        }
    }
}
