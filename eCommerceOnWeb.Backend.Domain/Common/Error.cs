namespace eCommerceOnWeb.Backend.Domain.Common
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "Значение не может быть пустым.");
    }
}
