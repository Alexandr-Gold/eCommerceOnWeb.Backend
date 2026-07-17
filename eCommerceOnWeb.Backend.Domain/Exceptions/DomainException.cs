namespace eCommerceOnWeb.Backend.Domain.Exceptions
{
    public class DomainException : Exception
    {
        // Базовый конструктор, принимающий человекочитаемый текст ошибки
        public DomainException(string message) : base(message)
        {
        }
    }
}
