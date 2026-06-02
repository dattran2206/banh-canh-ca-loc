using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;

namespace BanhCanhCaLoc.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICommand && 
                !typeof(TRequest).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            {
                return await next();
            }

            var response = await next();

            if (response is Result result && result.IsSuccess)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (response != null && response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(Result<>))
            {
                var isSuccessProp = response.GetType().GetProperty("IsSuccess");
                if (isSuccessProp != null && (bool)isSuccessProp.GetValue(response)!)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            return response;
        }
    }
}
