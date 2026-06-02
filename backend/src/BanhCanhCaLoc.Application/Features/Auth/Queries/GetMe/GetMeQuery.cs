using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Auth.Queries.GetMe
{
    public record GetMeQuery(Guid UserId) : IQuery<Result<User>>;

    public class GetMeQueryHandler : IQueryHandler<GetMeQuery, Result<User>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMeQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<User>> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<User>(new Error("User.NotFound", "Không tìm thấy người dùng"));
            }

            return user;
        }
    }
}
