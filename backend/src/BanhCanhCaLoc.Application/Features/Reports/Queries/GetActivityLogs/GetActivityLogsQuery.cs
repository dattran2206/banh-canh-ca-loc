using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Reports.Queries.GetActivityLogs
{
    public record GetActivityLogsQuery() : IQuery<Result<IReadOnlyList<ActivityLog>>>;

    public class GetActivityLogsQueryHandler : IQueryHandler<GetActivityLogsQuery, Result<IReadOnlyList<ActivityLog>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetActivityLogsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<ActivityLog>>> Handle(GetActivityLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _unitOfWork.Repository<ActivityLog, Guid>().GetAllAsync(cancellationToken);
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            var userMap = users.ToDictionary(u => u.Id);

            foreach (var log in logs)
            {
                if (log.UserId.HasValue && userMap.TryGetValue(log.UserId.Value, out var user))
                {
                    log.User = user;
                }
            }

            var orderedLogs = logs.OrderByDescending(l => l.Timestamp).Take(100).ToList();
            return Result.Success<IReadOnlyList<ActivityLog>>(orderedLogs);
        }
    }
}
