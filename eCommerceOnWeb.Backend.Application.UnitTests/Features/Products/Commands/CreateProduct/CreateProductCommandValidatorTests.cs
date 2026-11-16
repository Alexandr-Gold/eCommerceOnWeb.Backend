using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using FluentAssertions;
using FluentValidation.Results;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.UnitTests.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldBeValid_WhenCommandIsCorrect()
    {
        // Arrange
        CreateProductCommand command = CreateProductCommandTestData.ValidCreateCommand();

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Name = ""
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);


        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .ContainSingle(x =>
                x.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSkuIsEmpty()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Sku = " "
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenGtinHasInvalidFormat()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Gtin = "12345"
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.Gtin));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPriceIsNegative()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                PriceAmount = -1
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);


        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.PriceAmount));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCurrencyIsUnsupported()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                PriceCurrency = "GBP"
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.PriceCurrency));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenStockIsNegative()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                InitialStock = -1
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.InitialStock));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAttributeKeysAreDuplicated()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Attributes =
                [
                    new ProductAttributeDto(
                        "Color",
                        JsonDocument.Parse("\"Black\"").RootElement),

                    new ProductAttributeDto(
                        "color",
                        JsonDocument.Parse("\"White\"").RootElement)
                ]
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName ==
                nameof(CreateProductCommand.Attributes));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAttributeValueIsNull()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Attributes =
                [
                    new ProductAttributeDto(
                        "Color",
                        JsonDocument.Parse("null").RootElement)
                ]
            };

        // Act
        ValidationResult result =
             await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName.Contains(nameof(CreateProductCommand.Attributes)));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDimensionsAreInvalid()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                DimWidthCm = 0
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors
            .Should()
            .Contain(x =>
                x.PropertyName == nameof(CreateProductCommand.DimWidthCm));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPriceHasMoreThanTwoDecimalPlaces()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                PriceAmount = 99.999m
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);


        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .Contain(x =>
                x.PropertyName ==
                nameof(CreateProductCommand.PriceAmount));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAttributesAreNull()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Attributes = null!
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .Contain(x =>
                x.PropertyName ==
                nameof(CreateProductCommand.Attributes));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAttributeKeyIsEmpty()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Attributes =
                [
                    new ProductAttributeDto(
                    "",
                    JsonSerializer.SerializeToElement("Black"))
                ]
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .Contain(x =>
                x.PropertyName.Contains("Attributes"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAttributeValueIsObject()
    {
        // Arrange
        CreateProductCommand command =
            CreateProductCommandTestData.ValidCreateCommand() with
            {
                Attributes =
                [
                    new ProductAttributeDto(
                    "Specs",
                    JsonSerializer.SerializeToElement(
                        new { Cpu = "M4" }))
                ]
            };

        // Act
        ValidationResult result =
            await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}