using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Menu.Queries.GetCategories
{
    public record GetCategoriesQuery() : IQuery<Result<IReadOnlyList<Category>>>;

    public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, Result<IReadOnlyList<Category>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Category>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Repository<Category, int>().GetAllAsync(cancellationToken);
            return Result.Success(categories);
        }
    }
}
