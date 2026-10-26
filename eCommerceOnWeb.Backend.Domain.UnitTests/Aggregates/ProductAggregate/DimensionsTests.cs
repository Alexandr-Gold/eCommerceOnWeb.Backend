using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate
{
    public class DimensionsTests
    {
        public void Create_ShouldCreateDimensions_WhenArgumentsAreValid()
        {
            // Arrange
            decimal width = 20m;
            decimal height = 30m;
            decimal depth = 10m;
            decimal weight = 2.5m;

            // Act
            Dimensions dimensions = Dimensions.Create(
                width,
                height,
                depth,
                weight);

            // Assert
            dimensions.WidthCm.Should().Be(width);
            dimensions.HeightCm.Should().Be(height);
            dimensions.DepthCm.Should().Be(depth);
            dimensions.WeightKg.Should().Be(weight);
        }

        [Theory]
        [InlineData(0, 30, 10, 2)]
        [InlineData(20, 0, 10, 2)]
        [InlineData(20, 30, 0, 2)]
        [InlineData(20, 30, 10, 0)]
        public void Create_ShouldThrowDomainException_WhenAnyDimensionIsZero(
           decimal width,
           decimal height,
           decimal depth,
           decimal weight)
        {
            // Arrange
            Action act = () => Dimensions.Create(
                width,
                height,
                depth,
                weight);

            // Act & Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Габариты и вес для доставки электроники должны быть больше нуля.");
        }

        [Theory]
        [InlineData(-1, 30, 10, 2)]
        [InlineData(20, -1, 10, 2)]
        [InlineData(20, 30, -1, 2)]
        [InlineData(20, 30, 10, -1)]
        public void Create_ShouldThrowDomainException_WhenAnyDimensionIsNegative(
            decimal width,
            decimal height,
            decimal depth,
            decimal weight)
        {
            // Arrange
            Action act = () => Dimensions.Create(
                width,
                height,
                depth,
                weight);

            // Act & Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Габариты и вес для доставки электроники должны быть больше нуля.");
        }

        [Fact]
        public void Dimensions_ShouldBeEqual_WhenValuesAreEqual()
        {
            // Arrange
            Dimensions first = Dimensions.Create(
                20,
                30,
                10,
                2);

            Dimensions second = Dimensions.Create(
                20,
                30,
                10,
                2);

            // Assert
            first.Should().Be(second);
        }

        [Fact]
        public void Dimensions_ShouldNotBeEqual_WhenValuesAreDifferent()
        {
            // Arrange
            Dimensions first = Dimensions.Create(
                20,
                30,
                10,
                2);

            Dimensions second = Dimensions.Create(
                20,
                30,
                10,
                3);

            // Assert
            first.Should().NotBe(second);
        }
    }
}


