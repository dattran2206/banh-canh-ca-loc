using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using BanhCanhCaLoc.Application.Common.Models;

namespace BanhCanhCaLoc.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var valueType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result<>)
                        .MakeGenericType(valueType)
                        .GetMethod("Failure", new[] { typeof(Error) });

                    var firstError = new Error("Validation.Error", failures.First().ErrorMessage);
                    if (failureMethod != null)
                    {
                        return (TResponse)failureMethod.Invoke(null, new object[] { firstError })!;
                    }
                }
                else if (typeof(TResponse) == typeof(Result))
                {
                    var firstError = new Error("Validation.Error", failures.First().ErrorMessage);
                    return (Result.Failure(firstError) as TResponse)!;
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
