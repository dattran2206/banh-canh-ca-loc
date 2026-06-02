using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Shop.Commands.UpdateShopInfo
{
    public record UpdateShopInfoCommand(string Name, string Address, string Phone) : ICommand<Result<ShopInfo>>;

    public class UpdateShopInfoCommandValidator : AbstractValidator<UpdateShopInfoCommand>
    {
        public UpdateShopInfoCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên cửa hàng không được để trống")
                               .MaximumLength(150).WithMessage("Tên cửa hàng tối đa 150 ký tự");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Địa chỉ không được để trống")
                                  .MaximumLength(250).WithMessage("Địa chỉ tối đa 250 ký tự");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Số điện thoại không được để trống")
                                .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự");
        }
    }

    public class UpdateShopInfoCommandHandler : ICommandHandler<UpdateShopInfoCommand, Result<ShopInfo>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShopInfoCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ShopInfo>> Handle(UpdateShopInfoCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Repository<ShopInfo, int>().FirstOrDefaultAsync(_ => true, cancellationToken);
            if (existing == null)
            {
                existing = new ShopInfo
                {
                    Name = request.Name,
                    Address = request.Address,
                    Phone = request.Phone
                };
                await _unitOfWork.Repository<ShopInfo, int>().AddAsync(existing, cancellationToken);
            }
            else
            {
                existing.Name = request.Name;
                existing.Address = request.Address;
                existing.Phone = request.Phone;
                _unitOfWork.Repository<ShopInfo, int>().Update(existing);
            }

            return existing;
        }
    }
}
