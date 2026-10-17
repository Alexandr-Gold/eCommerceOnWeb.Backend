using eCommerceOnWeb.Backend.Application.Common.Exceptions;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceOnWeb.Backend.WebApi.Common.Exceptions
{
    public sealed class GlobalExceptionHandler
        : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
             HttpContext httpContext,
             Exception exception,
             CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred.");

            ProblemDetails problemDetails = exception switch
            {
                ValidationException validationException =>
                    CreateValidationProblem(validationException),

                NotFoundException notFoundException =>
                    CreateNotFoundProblem(notFoundException),

                DomainException domainException =>
                    CreateDomainProblem(domainException),

                _ =>
                    CreateInternalServerProblem()
            };

            httpContext.Response.StatusCode =
                problemDetails.Status ??
                StatusCodes.Status500InternalServerError;

            //httpContext.Response.ContentType =
            // "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(
                 problemDetails,
                 problemDetails.GetType(),
                 cancellationToken);

            return true;
        }

        private static ValidationProblemDetails CreateValidationProblem(
            ValidationException exception)
        {
            return new ValidationProblemDetails(
                new Dictionary<string, string[]>(exception.Errors))
            {
                Title = "Validation failed.",
                Status = StatusCodes.Status400BadRequest
            };
        }

        private static ProblemDetails CreateNotFoundProblem(
            NotFoundException exception)
        {
            return new ProblemDetails
            {
                Title = "Resource not found.",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound
            };
        }

        private static ProblemDetails CreateDomainProblem(
            DomainException exception)
        {
            return new ProblemDetails
            {
                Title = "Business rule violation.",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            };
        }

        private static ProblemDetails CreateInternalServerProblem()
        {
            return new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            };
        }
    }
}