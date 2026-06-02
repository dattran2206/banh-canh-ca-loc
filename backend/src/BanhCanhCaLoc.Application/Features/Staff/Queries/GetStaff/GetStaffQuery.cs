using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Staff.Queries.GetStaff
{
    public record GetStaffQuery() : IQuery<Result<IReadOnlyList<User>>>;

    public class GetStaffQueryHandler : IQueryHandler<GetStaffQuery, Result<IReadOnlyList<User>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<User>>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
        {
            var staff = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            return Result.Success(staff);
        }
    }
}
