using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;


namespace eCommerceOnWeb.Backend.Domain.UnitTests.Common
{
    public class MoneyTests
    {
        [Fact]
        public void Money_ShouldBeEqual_WhenValuesAreEqual()
        {
            // Arrange
            Money first = Money.Create(100m, "USD");
            Money second = Money.Create(100m, "USD");

            // Assert
            first.Should().Be(second);
        }

        [Fact]
        public void Money_ShouldNotBeEqual_WhenValuesAreDifferent()
        {
            // Arrange
            Money first = Money.Create(100m, "USD");
            Money second = Money.Create(200m, "USD");

            // Assert
            first.Should().NotBe(second);
        }

        [Fact]
        public void Zero_ShouldReturnMoneyWithZeroAmountAndDefaultCurrency()
        {
            // Act
            Money money = Money.Zero();

            // Assert
            money.Amount.Should().Be(0m);
            money.Currency.Should().Be("USD");
        }

        [Fact]
        public void Create_ShouldCreateMoney_WhenArgumentsAreValid()
        {
            // Arrange
            decimal amount = 100m;
            string currency = "USD";

            // Act
            Money money = Money.Create(amount, currency);

            // Assert
            money.Amount.Should().Be(amount);
            money.Currency.Should().Be(currency);
        }

        [Theory]
        [InlineData("usd", "USD")]
        [InlineData("Usd", "USD")]
        [InlineData("uSd", "USD")]
        [InlineData("eur", "EUR")]
        [InlineData("rub", "RUB")]
        public void Create_ShouldNormalizeCurrencyToUpperCase(
     string inputCurrency,
     string expectedCurrency)
        {
            // Arrange
            decimal amount = 100m;

            // Act
            Money money = Money.Create(amount, inputCurrency);

            // Assert
            money.Currency.Should().Be(expectedCurrency);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.01)]
        [InlineData(10.50)]
        [InlineData(999999.99)]
        public void Create_ShouldCreateMoney_WhenAmountHasMaximumAllowedPrecision(
   decimal amount)
        {
            // Act
            Money money = Money.Create(amount, "USD");

            // Assert
            money.Amount.Should().Be(amount);
        }

        [Fact]
        public void Create_ShouldThrowDomainException_WhenAmountIsNegative()
        {
            // Arrange
            Action act = () => Money.Create(-1m, "USD");

            // Act & Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Сумма не может быть отрицательной.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Create_ShouldThrowDomainException_WhenCurrencyIsNullOrWhiteSpace(
   string? currency)
        {
            // Arrange
            Action act = () => Money.Create(100m, currency!);

            // Act & Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Код валюты не может быть пустым.");
        }

        [Theory]
        [InlineData("GBP")]
        [InlineData("JPY")]
        [InlineData("CHF")]
        public void Create_ShouldThrowDomainException_WhenCurrencyIsUnsupported(
            string currency)
        {
            // Arrange
            Action act = () => Money.Create(100m, currency);

            // Act & Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage($"Валюта '{currency}' не поддерживается.");
        }

        [Theory]
        [InlineData(10.001)]
        [InlineData(99.999)]
        [InlineData(123.456)]
        public void Create_ShouldThrowDomainException_WhenAmountHasMoreThanTwoDecimalPlaces(
    decimal amount)
        {
            Action act = () => Money.Create(amount, "USD");

            act.Should()
                .Throw<DomainException>()
                .WithMessage("Сумма может содержать не более двух знаков после запятой.");
        }
    }
}
