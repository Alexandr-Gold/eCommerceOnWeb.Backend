namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Exceptions
{
    // Базовый класс для всех ошибок домена
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
