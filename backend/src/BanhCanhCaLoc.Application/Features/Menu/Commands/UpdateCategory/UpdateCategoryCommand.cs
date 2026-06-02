using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(int Id, string Name) : ICommand<Result<Category>>;

    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id danh mục không hợp lệ");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên danh mục không được để trống")
                               .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự");
        }
    }

    public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Result<Category>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Repository<Category, int>().GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<Category>(new Error("Category.NotFound", "Không tìm thấy danh mục"));
            }

            existing.Name = request.Name;

            _unitOfWork.Repository<Category, int>().Update(existing);

            return existing;
        }
    }
}
