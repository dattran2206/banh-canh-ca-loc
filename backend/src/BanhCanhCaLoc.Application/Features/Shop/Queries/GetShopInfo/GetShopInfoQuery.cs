using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Shop.Queries.GetShopInfo
{
    public record GetShopInfoQuery() : IQuery<Result<ShopInfo>>;

    public class GetShopInfoQueryHandler : IQueryHandler<GetShopInfoQuery, Result<ShopInfo>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetShopInfoQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ShopInfo>> Handle(GetShopInfoQuery request, CancellationToken cancellationToken)
        {
            var info = await _unitOfWork.Repository<ShopInfo, int>().FirstOrDefaultAsync(_ => true, cancellationToken);
            if (info == null)
            {
                return Result.Failure<ShopInfo>(new Error("Shop.NotFound", "Không tìm thấy thông tin cửa hàng"));
            }

            return info;
        }
    }
}
