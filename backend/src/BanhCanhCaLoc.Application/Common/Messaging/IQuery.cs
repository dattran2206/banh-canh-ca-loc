using MediatR;

namespace BanhCanhCaLoc.Application.Common.Messaging
{
    public interface IQuery<TResponse> : IRequest<TResponse>
    {
    }
}
