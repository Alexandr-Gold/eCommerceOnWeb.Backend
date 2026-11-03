using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate
{
    public class ProductImageTests
    {
        #region Constructor

        [Fact]
        public void Constructor_ShouldCreateProductImage_WhenParametersAreValid()
        {
            // Arrange
            const string storageKey = "products/image1.jpg";
            const string altText = "Product image";
            const int displayOrder = 1;

            // Act
            ProductImage image = new(
                storageKey,
                altText,
                displayOrder);

            // Assert
            image.StorageKey.Should().Be(storageKey);
            image.AltText.Should().Be(altText);
            image.DisplayOrder.Should().Be(displayOrder);
            image.IsMain.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ShouldTrimWhitespaceFromStrings()
        {
            // Arrange
            const string storageKey = "  products/image1.jpg  ";
            const string altText = "  Product image  ";

            // Act
            ProductImage image = new(
                storageKey,
                altText,
                0);

            // Assert
            image.StorageKey.Should()
                .Be("products/image1.jpg");

            image.AltText.Should()
                .Be("Product image");
        }

        #endregion

        #region UpdateDetails

        [Fact]
        public void UpdateDetails_ShouldModifyFields_WhenParametersAreValid()
        {
            // Arrange
            ProductImage image = new(
                "products/image.jpg",
                "Old text",
                1);

            // Act
            image.UpdateDetails(
                "  New text  ",
                5);

            // Assert
            image.AltText.Should()
                .Be("New text");

            image.DisplayOrder.Should()
                .Be(5);
        }

        [Fact]
        public void UpdateDetails_ShouldNotChangeStorageKey()
        {
            // Arrange
            ProductImage image = new(
                "products/image.jpg",
                "Old text",
                1);

            // Act
            image.UpdateDetails(
                "New text",
                5);

            // Assert
            image.StorageKey.Should()
                .Be("products/image.jpg");
        }

        #endregion

        #region Constructor validation

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowDomainException_WhenStorageKeyIsInvalid(
            string? invalidStorageKey)
        {
            // Act
            Action act = () =>
                new ProductImage(
                    invalidStorageKey!,
                    "Valid alt",
                    0);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Storage Key изображения обязателен.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowDomainException_WhenAltTextIsInvalid(
            string? invalidAltText)
        {
            // Act
            Action act = () =>
                new ProductImage(
                    "products/image.jpg",
                    invalidAltText!,
                    0);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Alt Text не может быть пустым.");
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenDisplayOrderIsNegative()
        {
            // Act
            Action act = () =>
                new ProductImage(
                    "products/image.jpg",
                    "Valid alt",
                    -1);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Display Order не может быть отрицательным.");
        }

        #endregion

        #region Update validation

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateDetails_ShouldThrowDomainException_WhenAltTextIsInvalid(
            string? invalidAltText)
        {
            // Arrange
            ProductImage image = new(
                "products/image.jpg",
                "Valid alt",
                0);

            // Act
            Action act = () =>
                image.UpdateDetails(
                    invalidAltText!,
                    1);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Alt Text не может быть пустым.");
        }

        [Fact]
        public void UpdateDetails_ShouldThrowDomainException_WhenDisplayOrderIsNegative()
        {
            // Arrange
            ProductImage image = new(
                "products/image.jpg",
                "Valid alt",
                0);

            // Act
            Action act = () =>
                image.UpdateDetails(
                    "New alt",
                    -5);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Display Order не может быть отрицательным.");
        }

        #endregion
    }
}
