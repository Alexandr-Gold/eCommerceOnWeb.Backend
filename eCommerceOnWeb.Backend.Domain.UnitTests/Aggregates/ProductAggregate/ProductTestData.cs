using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;

internal static class ProductTestData
{
    /// <summary>
    /// Строит товар в заданном статусе.
    /// Дефолт — Draft с описанием, ценой 999.99, картинкой и остатком 10.
    /// </summary>
    /// <remarks>
    /// Статус применяется через доменные методы (Activate/Deactivate/Discontinue),
    /// а не «в лоб» — это гарантирует, что все guard'ы пройдены корректно.
    ///
    /// После сборки DomainEvents очищаются: тесты проверяют события
    /// только от собственного Act, а не от Arrange.
    /// </remarks>
    public static Product CreateValidProduct(
        string name = "Test Smartphone",
        string sku = "TEST-001",
        string? gtin = "1234567890123",
        string modelNumber = "MODEL-001",
        decimal priceAmount = 999.99m,
        string currency = "USD",
        Guid? categoryId = null,
        Guid? brandId = null,
        string? description = "Test product description",
        int warrantyMonths = 24,
        decimal widthCm = 10m,
        decimal heightCm = 20m,
        decimal depthCm = 5m,
        decimal weightKg = 1m,
        ProductStatus status = ProductStatus.Draft,
        bool withImage = true,
        bool isDeleted = false,
        int initialStock = 10)
    {
        Product product = Product.Create(
            name: name,
            sku: sku,
            gtin: gtin,
            modelNumber: modelNumber,
            price: Money.Create(priceAmount, currency),
            categoryId: categoryId ?? Guid.NewGuid(),
            brandId: brandId ?? Guid.NewGuid(),
            description: description,
            warrantyMonths: warrantyMonths,
            shippingDimensions: Dimensions.Create(widthCm, heightCm, depthCm, weightKg),
            initialStock: initialStock);

        if (withImage)
            product.AddImage("test-storage-key", "test alt", displayOrder: 1);

        ApplyStatus(product, status);

        if (isDeleted)
        {
            if (product.StockQuantity > 0)
                throw new InvalidOperationException(
                    "Нельзя создать удалённый товар с остатком на складе. " +
                    "Укажите initialStock: 0 или isDeleted: false.");

            product.Delete();
        }

        // Тесты проверяют события только от Act, не от Arrange.
        product.ClearDomainEvents();

        return product;
    }

    private static void ApplyStatus(Product product, ProductStatus status)
    {
        switch (status)
        {
            case ProductStatus.Draft:
                break;

            case ProductStatus.Active:
                product.Activate();
                break;

            case ProductStatus.Inactive:
                product.Activate();
                product.Deactivate();
                break;

            case ProductStatus.Discontinued:
                product.Discontinue();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status), status, "Неизвестный статус.");
        }
    }
}