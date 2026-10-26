using eCommerceOnWeb.Backend.Application.Common.Exceptions;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            switch (exception)
            {
                // Оставляем только базовый тип, он поймает и TaskCanceledException
                case OperationCanceledException:
                    _logger.LogInformation(
                        "Запрос к {Path} был отменен пользователем.",
                        httpContext.Request.Path);

                    return true; // Сразу выходим, писать в Response отмененного запроса нельзя

                case ValidationException:
                    _logger.LogWarning(
                        "Ошибка валидации запроса для {Path}",
                        httpContext.Request.Path);
                    break;

                case DomainException:
                    _logger.LogWarning(
                        "Нарушение бизнес-правила при обработке {Path}",
                        httpContext.Request.Path);
                    break;

                // Специфичный NotFoundException должен идти ДО базового DomainException
                case NotFoundException:
                    _logger.LogInformation(
                        "Запрошенный ресурс не найден: {Path}",
                        httpContext.Request.Path);
                    break;

                default:
                    _logger.LogError(
                        exception,
                        "Необработанная ошибка при выполнении запроса {Path}",
                        httpContext.Request.Path);
                    break;
            }

            ProblemDetails problemDetails = exception switch
            {
                ValidationException validationException =>
                    CreateValidationProblem(validationException),

                NotFoundException notFoundException =>
                    CreateNotFoundProblem(notFoundException),

                DomainException domainException =>
                    CreateDomainProblem(domainException),

                DbUpdateException dbUpdateException =>
                    CreateDatabaseProblem(dbUpdateException),
                _ =>
                    CreateInternalServerProblem()
            };

            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            httpContext.Response.StatusCode =
                problemDetails.Status ??
                StatusCodes.Status500InternalServerError;

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
                Title = "Internal server error.",
                Status = StatusCodes.Status500InternalServerError
            };
        }

        private static ProblemDetails CreateDatabaseProblem(
          DbUpdateException exception)
        {
            PostgresException? postgresException =
                exception.GetBaseException() as PostgresException;

            if (postgresException != null)
            {
                if (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return postgresException.ConstraintName switch
                    {
                        "IX_products_sku" =>
                            new ProblemDetails
                            {
                                Title = "Unique constraint violation.",
                                Detail = "Товар с таким SKU уже существует.",
                                Status = StatusCodes.Status409Conflict
                            },

                        "IX_products_gtin" =>
                            new ProblemDetails
                            {
                                Title = "Unique constraint violation.",
                                Detail = "Товар с таким GTIN уже существует.",
                                Status = StatusCodes.Status409Conflict
                            },

                        _ =>
                            new ProblemDetails
                            {
                                Title = "Unique constraint violation.",
                                Detail = "Объект с такими уникальными данными уже существует.",
                                Status = StatusCodes.Status409Conflict
                            }
                    };
                }

                if (postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation)
                {
                    return new ProblemDetails
                    {
                        Title = "Foreign key constraint violation.",
                        Detail = "Связанный объект не существует.",
                        Status = StatusCodes.Status400BadRequest
                    };
                }
            }

            return new ProblemDetails
            {
                Title = "Database update failed.",
                Detail = "Во время обработки запроса произошла внутренняя ошибка сервера.",
                Status = StatusCodes.Status500InternalServerError
            };
        }
    }
}
