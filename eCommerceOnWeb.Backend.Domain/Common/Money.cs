using eCommerceOnWeb.Backend.Domain.Exceptions;
using System.Text.Json.Serialization;

namespace eCommerceOnWeb.Backend.Domain.Common;

public sealed record Money
{
    private const string DefaultCurrency = "USD";

    private static readonly HashSet<string> SupportedCurrencies =
        new(StringComparer.Ordinal)
        {
        "USD",
        "EUR",
        "RUB"
        };

    public decimal Amount { get; }
    public string Currency { get; }

    /// <summary>
    /// Единственный способ создать Money — конструктор валидирует инварианты:
    /// сумма неотрицательна, scale ≤ 2, валюта из поддерживаемого списка.
    /// Обход через <c>new Money(-5, "USD")</c> невозможен: валидация в теле ctor.
    /// </summary>
    /// <exception cref="DomainException">Нарушен любой из инвариантов.</exception>
    [JsonConstructor]
    public Money(decimal amount, string? currency)
    {
        if (amount < 0)
            throw new DomainException(
                "Сумма не может быть отрицательной.");

        // decimal.GetBits(...)[3] >> 16 & 0xFF — scale (число знаков после запятой).
        // Strict: 100.000m отвергается, хотя value == 100.00m.
        // Если продукт решит «нормализовать, а не бросать» — заменить на
        //   amount = decimal.Round(amount, 2);
        //   if (decimal.Round(amount, 2) != amount) throw ...;

        int scale = (decimal.GetBits(amount)[3] >> 16) & 0xFF;
        if (scale > 2)
            throw new DomainException(
                "Сумма может содержать не более двух знаков после запятой.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException(
                "Код валюты не может быть пустым.");

        string normalizedCurrency = currency.ToUpperInvariant();
        if (!SupportedCurrencies.Contains(normalizedCurrency))
            throw new DomainException(
                $"Валюта '{currency}' не поддерживается.");

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public static Money Zero() => new(0, DefaultCurrency);

    /// <summary>Семантический алиас для <c>new Money(...)</c>. Валидация — в ctor.</summary>
    public static Money Create(
        decimal amount,
        string? currency) => new(amount, currency);

    public bool IsZero => Amount == 0m;
    public bool IsPositive => Amount > 0m;

    public static bool IsSupportedCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        return SupportedCurrencies.Contains(
            currency.Trim().ToUpperInvariant());
    }
}

