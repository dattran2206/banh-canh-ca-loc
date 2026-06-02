using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Auth.Queries.GetShifts
{
    public record GetShiftsQuery(Guid UserId, string Role) : IQuery<Result<IReadOnlyList<Shift>>>;

    public class GetShiftsQueryHandler : IQueryHandler<GetShiftsQuery, Result<IReadOnlyList<Shift>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetShiftsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<Shift>>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<Shift> shifts;
            if (request.Role == "admin")
            {
                shifts = await _unitOfWork.Repository<Shift, Guid>().GetAllAsync(cancellationToken);
            }
            else
            {
                shifts = await _unitOfWork.Repository<Shift, Guid>().GetAsync(s => s.UserId == request.UserId, cancellationToken);
            }

            var orderedShifts = shifts.OrderByDescending(s => s.StartTime).Take(50).ToList();

            return orderedShifts;
        }
    }
}
