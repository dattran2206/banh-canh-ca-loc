using MediatR;
using BanhCanhCaLoc.Application.Common.Models;

namespace BanhCanhCaLoc.Application.Common.Messaging
{
    public interface ICommand : IRequest<Result>
    {
    }

    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }
}
