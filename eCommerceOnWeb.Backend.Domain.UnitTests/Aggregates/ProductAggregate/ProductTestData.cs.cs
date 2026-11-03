using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate
{
    internal static class ProductTestData
    {
        public static Product CreateValidProduct(
            string name = "Test Smartphone",
            string sku = "TEST-001",
            string? gtin = "1234567890123",
            string modelNumber = "MODEL-001",
            decimal priceAmount = 999.99m,
            string currency = "USD",
            Guid? categoryId = null,
            Guid? brandId = null,
            string description = "Test product description",
            int warrantyMonths = 24,
            decimal widthCm = 10m,
            decimal heightCm = 20m,
            decimal depthCm = 5m,
            decimal weightKg = 1m,
            bool IsActive = true,
            bool IsDeleted = false,
            int initialStock = 10)
        {
            Guid finalCategoryId =
                categoryId ?? Guid.NewGuid();

            Guid finalBrandId =
                brandId ?? Guid.NewGuid();

            return Product.Create(
                name: name,
                sku: sku,
                gtin: gtin,
                modelNumber: modelNumber,
                price: Money.Create(priceAmount, currency),
                categoryId: finalCategoryId,
                brandId: finalBrandId,
                description: description,
                warrantyMonths: warrantyMonths,
                shippingDimensions: Dimensions.Create(
                    widthCm,
                    heightCm,
                    depthCm,
                    weightKg),
                initialStock: initialStock);
        }
    }
}
