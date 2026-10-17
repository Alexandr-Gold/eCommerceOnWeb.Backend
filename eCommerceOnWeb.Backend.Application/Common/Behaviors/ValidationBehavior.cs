using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ApplicationValidationException =
    eCommerceOnWeb.Backend.Application.Common.Exceptions.ValidationException;

namespace eCommerceOnWeb.Backend.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);

        ValidationFailure[] failures = (await Task.WhenAll(
                 _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
             .SelectMany(result => result.Errors)
             .Where(error => error != null)
             .Distinct()
             .ToArray();

        if (failures.Length != 0)
        {
            IReadOnlyDictionary<string, string[]> errors =
                failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(f => f.ErrorMessage)
                            .Distinct()
                            .ToArray());

            throw new ApplicationValidationException(errors);
        }

        return await next();
    }
}




