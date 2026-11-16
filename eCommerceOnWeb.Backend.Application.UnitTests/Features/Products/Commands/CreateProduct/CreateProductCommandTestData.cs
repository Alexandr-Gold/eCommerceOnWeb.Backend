using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.UnitTests;
internal static class CreateProductCommandTestData
{
    public static CreateProductCommand ValidCreateCommand()
    {
        return new CreateProductCommand(
            Name: "Samsung Galaxy S25 Ultra",
            Description: "Test smartphone",
            Sku: "SAM-S25-001",
            Gtin: "1234567890123",
            ModelNumber: "SM-S938B",
            PriceAmount: 999.99m,
            PriceCurrency: "USD",
            CategoryId: Guid.NewGuid(),
            BrandId: Guid.NewGuid(),
            WarrantyMonths: 24,
            DimWidthCm: 10,
            DimHeightCm: 20,
            DimDepthCm: 5,
            DimWeightKg: 1,
            InitialStock: 10,
            Attributes:
            [
                new ProductAttributeDto(
                    "Color",
                    JsonDocument.Parse("\"Black\"").RootElement)
            ]);
    }
}
