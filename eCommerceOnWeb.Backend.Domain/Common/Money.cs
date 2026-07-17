using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Common
{
    public record Money(decimal Amount, string Currency)
    {
        public static Money Zero() => new(0, "RUB");

        // Инвариант: цена не может быть отрицательной
        public static Money Create(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Сумма не может быть отрицательной.");

            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new DomainException("Некорректный ISO код валюты.");

            return new Money(amount, currency.ToUpperInvariant());
        }
    }
}
