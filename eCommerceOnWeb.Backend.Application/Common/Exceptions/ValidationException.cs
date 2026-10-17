using System.Collections.ObjectModel;

namespace eCommerceOnWeb.Backend.Application.Common.Exceptions
{
    /// <summary>
    /// Исключение, содержащее ошибки валидации входного запроса.
    /// Генерируется ValidationBehavior и обрабатывается GlobalExceptionHandler.
    /// </summary>
    public sealed class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(
            IReadOnlyDictionary<string, string[]> errors)
            : base("The request contains one or more validation errors.")
        {
            Errors = errors;
        }

        public ValidationException(
            string propertyName,
            string errorMessage)
            : this(new ReadOnlyDictionary<string, string[]>(
                new Dictionary<string, string[]>
                {
                    [propertyName] = new[] { errorMessage }
                }))
        {
        }
    }
}