using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;

public class ProductMoneyTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenAmountNegative()
    {
        Func<Money> act = () => new Money(-1m, "USD");
        act.Should().Throw<DomainException>().WithMessage("*отрицательной*");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenScaleAboveTwo()
    {
        Func<Money> act = () => new Money(100.001m, "USD");
        act.Should().Throw<DomainException>().WithMessage("*двух знаков*");
    }

    [Fact]
    public void Ctor_ShouldNormalizeCurrency()
    {
        Money m = new Money(100m, "usd");
        m.Currency.Should().Be("USD");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenCurrencyUnsupported()
    {
        Func<Money> act = () => new Money(100m, "GBP");
        act.Should().Throw<DomainException>().WithMessage("*не поддерживается*");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenCurrencyNull()
    {
        Func<Money> act = () => new Money(100m, null!);
        act.Should().Throw<DomainException>().WithMessage("*пустым*");
    }

    [Fact]
    public void NewAndCreate_ShouldBeEquivalent()
    {
        // Регрессия: раньше new Money(-5) обходил валидацию
        Func<Money> act1 = () => new Money(-5m, "USD");
        Func<Money> act2 = () => Money.Create(-5m, "USD");

        act1.Should().Throw<DomainException>();
        act2.Should().Throw<DomainException>();
    }

    [Fact]
    public void ValueEquality_ShouldCompareAmountAndCurrency()
    {
        Money a = new Money(100m, "USD");
        Money b = new Money(100m, "USD");
        Money c = new Money(100m, "EUR");

        a.Should().Be(b);          // == true
        a.Should().NotBe(c);       // == false
    }

    [Fact]
    public void IsPositive_ShouldReturnTrueForPositiveAmount()
    {
        new Money(0m, "USD").IsPositive.Should().BeFalse();
        new Money(0.01m, "USD").IsPositive.Should().BeTrue();
    }
}
