using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;
public class ProductCreationTests
{

    private static Product CreateValidProduct(
    string? name = "Test Smartphone",
    string? sku = "TEST-001",
    string? gtin = "1234567890123",
    string? modelNumber = "MODEL-001",
    Guid? categoryId = null,
    Guid? brandId = null,
    int warrantyMonths = 12,
    int initialStock = 5)
    {
        return Product.Create(
            name: name!,
            sku: sku!,
            gtin: gtin,
            modelNumber: modelNumber!,
            price: Money.Create(999.99m, "USD"),
            categoryId: categoryId ?? Guid.NewGuid(),
            brandId: brandId ?? Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: warrantyMonths,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: initialStock);
    }

    [Fact]
    public void Create_ShouldCreateProduct_WhenArgumentsAreValid()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid brandId = Guid.NewGuid();

        // Act
        Product product = Product.Create(
            name: "iPhone 15 Pro",
            sku: "IPH-15PRO-128",
            gtin: "1234567890123",
            modelNumber: "A3106",
            price: Money.Create(999.00m, "USD"),
            categoryId: categoryId,
            brandId: brandId,
            description: "Test product description",
            warrantyMonths: 12,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 5);

        // Assert
        product.Id.Should().NotBe(Guid.Empty);           // BaseEntity.Init
        product.Name.Should().Be("iPhone 15 Pro");
        product.Sku.Should().Be("IPH-15PRO-128");
        product.Gtin.Should().Be("1234567890123");
        product.ModelNumber.Should().Be("A3106");
        product.Price.Should().Be(Money.Create(999.00m, "USD"));
        product.CategoryId.Should().Be(categoryId);      // ← не проверялось
        product.BrandId.Should().Be(brandId);            // ← не проверялось
        product.Description.Should().Be("Test product description");   // ← не проверялось
        product.WarrantyMonths.Should().Be(12);
        product.StockQuantity.Should().Be(5);
        product.Status.Should().Be(ProductStatus.Draft);
        product.IsDeleted.Should().BeFalse();
        product.DeletedAtUtc.Should().BeNull();          // ← не проверялось
        product.Images.Should().BeEmpty();               // ← не проверялось: Create не добавляет картинок
        product.Attributes.GetValues().Should().BeEmpty();          // ← не проверялось
    }

    [Fact]
    public void Create_ShouldAddProductCreatedDomainEvent()
    {
        // Arrange: Create напрямую — хелпер очищает DomainEvents
        const string sku = "IPH-15PRO-128";

        Product product = Product.Create(
            name: "Test Smartphone",
            sku: sku,
            gtin: "1234567890123",
            modelNumber: "MODEL-001",
            price: Money.Create(999.99m, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: 24,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 10);

        // Act
        IReadOnlyCollection<DomainEvent> events = product.DomainEvents;

        // Assert
        events.Should().ContainSingle();

        ProductCreatedEvent domainEvent = events.Single()
            .Should().BeOfType<ProductCreatedEvent>()
            .Subject;

        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.Sku.Should().Be(sku);
        domainEvent.Price.Should().Be(product.Price);
    }

    [Fact]
    public void Create_ShouldInitializeStatusAsDraft()
    {
        // Arrange: Product.Create напрямую — статус Draft выставляется фабрикой,
        // а не хелпером через ApplyStatus.
        Product product = Product.Create(
            name: "Test Smartphone",
            sku: "TEST-001",
            gtin: null,
            modelNumber: "MODEL-001",
            price: Money.Create(999.99m, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: 24,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 10);

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Create_ShouldAcceptIdentityFieldsAtMaximumLength()
    {
        // Arrange
        string name = new(
            'N',
            ProductConstraints.NameMaxLength);

        string sku = new(
            's',
            ProductConstraints.SkuMaxLength);

        string modelNumber = new(
            'M',
            ProductConstraints.ModelNumberMaxLength);

        // Act
        Product product = CreateValidProduct(
            name: name,
            sku: sku,
            modelNumber: modelNumber);

        // Assert
        product.Name.Should().HaveLength(
            ProductConstraints.NameMaxLength);

        product.Sku.Should().HaveLength(
            ProductConstraints.SkuMaxLength);

        product.ModelNumber.Should().HaveLength(
            ProductConstraints.ModelNumberMaxLength);

        product.Sku.Should().Be(sku.ToUpperInvariant());
    }

    [Fact]
    public void Create_ShouldNormalizeIdentityFields()
    {
        // Arrange & Act
        Product product = CreateValidProduct(
            name: "  Test Smartphone  ",
            sku: "  test-001  ",
            gtin: " 1234567890123 ",
            modelNumber: "  MODEL-001  ");

        // Assert
        product.Name.Should().Be("Test Smartphone");
        product.Sku.Should().Be("TEST-001");
        product.Gtin.Should().Be("1234567890123");
        product.ModelNumber.Should().Be("MODEL-001");
    }
}
