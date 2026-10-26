using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate
{
    public class ProductAttributesTests
    {
        [Fact]
        public void Empty_ShouldCreateEmptyAttributes()
        {
            // Act
            ProductAttributes attributes = ProductAttributes.Empty();

            // Assert
            attributes.GetValues()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void SetValue_ShouldAddNewAttribute()
        {
            // Act
            ProductAttributes attributes =
                ProductAttributes.Empty()
                    .SetValue("Color", "Black");

            // Assert
            attributes.GetValue<string>("Color")
                .Should()
                .Be("Black");
        }

        [Fact]
        public void SetValue_ShouldNotModifyOriginalInstance()
        {
            // Arrange
            ProductAttributes original =
                ProductAttributes.Empty();

            // Act
            ProductAttributes updated =
                original.SetValue("RAM", 8);

            // Assert
            original.GetValues()
                .Should()
                .BeEmpty();

            updated.GetValue<int>("RAM")
                .Should()
                .Be(8);
        }

        [Fact]
        public void SetValue_ShouldReplaceExistingValue()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty()
                    .SetValue("Storage", "128GB");

            // Act
            ProductAttributes updated =
                attributes.SetValue("Storage", "256GB");

            // Assert
            updated.GetValue<string>("Storage")
                .Should()
                .Be("256GB");
        }

        [Theory]
        [InlineData("Text")]
        [InlineData(123)]
        [InlineData(123L)]
        [InlineData(99.99)]
        [InlineData(true)]
        public void SetValue_ShouldAcceptSupportedPrimitiveTypes(
            object value)
        {
            // Act
            ProductAttributes attributes =
                ProductAttributes.Empty()
                    .SetValue("Attribute", value);

            // Assert
            attributes.GetValues()
                .Should()
                .ContainKey("Attribute");
        }

        [Fact]
        public void GetValue_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty();

            // Act
            string? result =
                attributes.GetValue<string>("Unknown");

            // Assert
            result.Should()
                .BeNull();
        }

        [Fact]
        public void GetValue_ShouldReturnDefault_WhenTypeIsDifferent()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty()
                    .SetValue("Weight", 10);

            // Act
            string? result =
                attributes.GetValue<string>("Weight");

            // Assert
            result.Should()
                .BeNull();
        }

        [Fact]
        public void RemoveValue_ShouldRemoveExistingAttribute()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty()
                    .SetValue("Color", "Red");

            // Act
            ProductAttributes result =
                attributes.RemoveValue("Color");

            // Assert
            result.GetValues()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void FromDictionary_ShouldCopyValues()
        {
            // Arrange
            Dictionary<string, object> source = new()
            {
                ["Brand"] = "Apple"
            };

            // Act
            ProductAttributes attributes =
                ProductAttributes.FromDictionary(source);

            source["Brand"] = "Samsung";

            // Assert
            attributes.GetValue<string>("Brand")
                .Should()
                .Be("Apple");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void SetValue_ShouldThrow_WhenKeyIsEmpty(
            string? key)
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty();

            // Act
            Action act = () =>
                attributes.SetValue(key!, "value");

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Ключ атрибута не может быть пустым.");
        }


        [Fact]
        public void SetValue_ShouldThrow_WhenValueIsNull()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty();

            // Act
            Action act = () =>
                attributes.SetValue("Color", null!);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Значение атрибута 'Color' не может быть null.");
        }


        [Fact]
        public void SetValue_ShouldThrow_WhenValueTypeIsUnsupported()
        {
            // Arrange
            ProductAttributes attributes =
                ProductAttributes.Empty();

            object invalidValue = new object();

            // Act
            Action act = () =>
                attributes.SetValue(
                    "Invalid",
                    invalidValue);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Недопустимый тип значения для атрибута 'Invalid'.");
        }
    }
}