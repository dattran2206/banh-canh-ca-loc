using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanhCanhCaLoc.Application.Common.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyOrderUpdatedAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
