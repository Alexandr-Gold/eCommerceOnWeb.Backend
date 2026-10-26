using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Common
{
    public record Money(decimal Amount, string Currency)
    {
        private const string DefaultCurrency = "USD";

        private static readonly HashSet<string> SupportedCurrencies =
        [
            "USD",
            "EUR",
            "RUB"
        ];

        public static Money Zero() =>
            new(0, DefaultCurrency);

        // Инвариант: цена не может быть отрицательной,
        // валюта должна быть поддерживаемой.
        public static Money Create(
    decimal amount,
    string currency)
        {
            if (amount < 0)
                throw new DomainException(
                    "Сумма не может быть отрицательной.");

            int scale = (decimal.GetBits(amount)[3] >> 16) & 0xFF;

            if (scale > 2)
                throw new DomainException(
                    "Сумма может содержать не более двух знаков после запятой.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException(
                    "Код валюты не может быть пустым.");

            string normalizedCurrency =
                currency.ToUpperInvariant();

            if (!SupportedCurrencies.Contains(normalizedCurrency))
                throw new DomainException(
                    $"Валюта '{currency}' не поддерживается.");

            return new Money(
                amount,
                normalizedCurrency);
        }
    }
}
