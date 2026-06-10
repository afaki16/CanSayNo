using FluentValidation;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Any())
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Any())
                {
                    var errorModels = failures
                        .Select(x => Error.Failure(ErrorCode.ValidationFailed, x.ErrorMessage))
                        .ToList();

                    var resultType = typeof(TResponse);

                    if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        var dataType = resultType.GetGenericArguments()[0];
                        var method = typeof(Result<>)
                            .MakeGenericType(dataType)
                            .GetMethod(nameof(Result.Failure), new[] { typeof(IEnumerable<Error>) });

                        return (TResponse)method!.Invoke(null, [errorModels])!;
                    }

                    return (TResponse)typeof(Result)
                        .GetMethod(nameof(Result.Failure), new[] { typeof(IEnumerable<Error>) })!
                        .Invoke(null, [errorModels])!;
                }
            }

            return await next();
        }
    }
} 
