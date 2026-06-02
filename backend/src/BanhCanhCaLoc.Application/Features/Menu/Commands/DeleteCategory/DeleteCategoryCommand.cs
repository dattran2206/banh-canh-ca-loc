using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(int Id) : ICommand<Result>;

    public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Repository<Category, int>().GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Failure(new Error("Category.NotFound", "Không tìm thấy danh mục"));
            }

            var hasItems = await _unitOfWork.MenuItems
                .FirstOrDefaultAsync(m => m.CategoryId == request.Id, cancellationToken);

            if (hasItems != null)
            {
                return Result.Failure(new Error("Category.HasItems", "Danh mục đang chứa món ăn, không thể xóa."));
            }

            _unitOfWork.Repository<Category, int>().Delete(category);

            return Result.Success();
        }
    }
}
