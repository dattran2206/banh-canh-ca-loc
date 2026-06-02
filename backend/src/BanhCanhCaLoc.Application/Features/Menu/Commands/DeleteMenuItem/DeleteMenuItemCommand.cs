using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteMenuItem
{
    public record DeleteMenuItemCommand(int Id) : ICommand<Result>;

    public class DeleteMenuItemCommandHandler : ICommandHandler<DeleteMenuItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMenuItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _unitOfWork.MenuItems.GetByIdAsync(request.Id, cancellationToken);
            if (item == null)
            {
                return Result.Failure(new Error("MenuItem.NotFound", "Không tìm thấy món ăn"));
            }

            _unitOfWork.MenuItems.Delete(item);

            return Result.Success();
        }
    }
}
