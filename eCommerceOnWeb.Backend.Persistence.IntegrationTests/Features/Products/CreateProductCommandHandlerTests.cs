using eCommerceOnWeb.Backend.Application.Common.Exceptions;
using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Features.Products;
[Collection(PostgresTestCollection.Name)]
public sealed class CreateProductCommandHandlerTests
{
    private readonly PostgresTestDatabaseFixture _fixture;

    public CreateProductCommandHandlerTests(
        PostgresTestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnFailure_WhenSkuAlreadyExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        CreateProductCommand firstCommand = CreateValidCommand(
            sku: "DUP-SKU",
            gtin: "1234567890123");

        Domain.Common.Result<Guid> firstResult = await sender.Send(firstCommand);
        firstResult.IsSuccess.Should().BeTrue();

        CreateProductCommand secondCommand = CreateValidCommand(
            sku: "DUP-SKU",
            gtin: "9999999999999");

        Domain.Common.Result<Guid> secondResult = await sender.Send(secondCommand);

        secondResult.IsFailure.Should().BeTrue();
        secondResult.Error.Code.Should().Be("Products.SkuAlreadyExists");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnFailure_WhenGtinAlreadyExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        CreateProductCommand firstCommand = CreateValidCommand(
            sku: "SKU-100",
            gtin: "1234567890123");

        Domain.Common.Result<Guid> firstResult = await sender.Send(firstCommand);
        firstResult.IsSuccess.Should().BeTrue();

        CreateProductCommand secondCommand = CreateValidCommand(
            sku: "SKU-101",
            gtin: "1234567890123");

        Domain.Common.Result<Guid> secondResult = await sender.Send(secondCommand);

        secondResult.IsFailure.Should().BeTrue();
        secondResult.Error.Code.Should().Be("Products.GtinAlreadyExists");
    }

    [Fact]
    public async Task CreateProduct_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        CreateProductCommand command = CreateValidCommand(name: string.Empty);

        Func<Task<Domain.Common.Result<Guid>>> act = async () => await sender.Send(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProduct_ShouldThrowValidationException_WhenPriceIsZero()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        int beforeCount = await dbContext.Products.CountAsync();

        CreateProductCommand command = CreateValidCommand(priceAmount: 0m);

        Func<Task<Domain.Common.Result<Guid>>> act = async () => await sender.Send(command);

        await act.Should().ThrowAsync<ValidationException>();

        int afterCount = await dbContext.Products.CountAsync();
        afterCount.Should().Be(beforeCount);
    }

    [Fact]
    public async Task CreateProduct_ShouldThrowValidationException_WhenPriceIsNegative()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        int beforeCount = await dbContext.Products.CountAsync();

        CreateProductCommand command = CreateValidCommand(priceAmount: -1m);

        Func<Task<Domain.Common.Result<Guid>>> act = async () => await sender.Send(command);

        await act.Should().ThrowAsync<ValidationException>();

        int afterCount = await dbContext.Products.CountAsync();
        afterCount.Should().Be(beforeCount);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        CreateProductCommand command = CreateValidCommand(categoryId: Guid.NewGuid());

        Domain.Common.Result<Guid> result = await sender.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Products.CategoryNotFound");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnFailure_WhenBrandDoesNotExist()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        CreateProductCommand command = CreateValidCommand(brandId: Guid.NewGuid());

        Domain.Common.Result<Guid> result = await sender.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Products.BrandNotFound");
    }

    private CreateProductCommand CreateValidCommand(
    string? name = null,
    string? description = null,
    string? sku = null,
    string? gtin = "1234567890123",
    string? modelNumber = null,
    decimal priceAmount = 999.99m,
    string priceCurrency = "USD",
    Guid? categoryId = null,
    Guid? brandId = null,
    int warrantyMonths = 12,
    decimal dimWidthCm = 10m,
    decimal dimHeightCm = 20m,
    decimal dimDepthCm = 5m,
    decimal dimWeightKg = 1.5m,
    int initialStock = 10,
    IReadOnlyCollection<ProductAttributeDto>? attributes = null)
    {
        return new CreateProductCommand(
            name ?? "Test Product",
            description ?? "Test product description",
            sku ?? "TEST-SKU-001",
            gtin,
            modelNumber ?? "MODEL-001",
            priceAmount,
            priceCurrency,
            categoryId ?? _fixture.SeedData.CategoryId,
            brandId ?? _fixture.SeedData.BrandId,
            warrantyMonths,
            dimWidthCm,
            dimHeightCm,
            dimDepthCm,
            dimWeightKg,
            initialStock,
            attributes ?? Array.Empty<ProductAttributeDto>());
    }

    [Fact]
    public async Task CreateProduct_ShouldPersistJsonbAttributes_WhenAttributesAreProvided()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        CreateProductCommand command = CreateValidCommand(
            sku: "SKU-ATTR-001",
            gtin: "1234567890123",
            attributes: new[]
            {
            new ProductAttributeDto("ram", CreateJsonElement("16")),
            new ProductAttributeDto("color", CreateJsonElement("\"black\"")),
            new ProductAttributeDto("is_refurbished", CreateJsonElement("false"))
            });

        Domain.Common.Result<Guid> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        Product product = await dbContext.Products
            .AsNoTracking()
            .FirstAsync(x => x.Id == result.Value);

        product.Attributes.Should().NotBeNull();

        product.Attributes.TryGetValue<int>("ram", out int ram).Should().BeTrue();
        ram.Should().Be(16);

        product.Attributes.TryGetValue<string>("color", out string? color).Should().BeTrue();
        color.Should().Be("black");

        product.Attributes.TryGetValue<bool>("is_refurbished", out bool isRefurbished).Should().BeTrue();
        isRefurbished.Should().BeFalse();
    }

    private static JsonElement CreateJsonElement(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
    //==============================================================

    [Fact]
    public async Task CreateProduct_ShouldPersistProduct_WhenCommandIsValid()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        CreateProductCommand command = CreateValidCommand(
            sku: "SKU-001",
            gtin: "1234567890123");

        Domain.Common.Result<Guid> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        Product product = await dbContext.Products
            .AsNoTracking()
            .FirstAsync(x => x.Id == result.Value);

        product.Name.Should().Be(command.Name);
        product.Sku.Should().Be(command.Sku.ToUpperInvariant());
        product.Gtin.Should().Be(command.Gtin);
        product.ModelNumber.Should().Be(command.ModelNumber);
        product.CategoryId.Should().Be(_fixture.SeedData.CategoryId);
        product.BrandId.Should().Be(_fixture.SeedData.BrandId);
        product.Description.Should().Be(command.Description);
        product.WarrantyMonths.Should().Be(command.WarrantyMonths);
        product.StockQuantity.Should().Be(command.InitialStock);
        product.Price.Amount.Should().Be(command.PriceAmount);
        product.Price.Currency.Should().Be(command.PriceCurrency);

        product.ShippingDimensions.Should().NotBeNull();
        product.ShippingDimensions.WidthCm.Should().Be(command.DimWidthCm);
        product.ShippingDimensions.HeightCm.Should().Be(command.DimHeightCm);
        product.ShippingDimensions.DepthCm.Should().Be(command.DimDepthCm);
        product.ShippingDimensions.WeightKg.Should().Be(command.DimWeightKg);
        product.Status.Should().Be(ProductStatus.Draft);
    }

    //[Fact]
    //public async Task CreateProduct_ShouldPersistOnlyOneRow_WhenTwoRequestsUseSameSkuConcurrently()
    //{
    //    await _fixture.ResetDatabaseAsync();

    //    await using AsyncServiceScope scope1 = _fixture.ServiceProvider.CreateScope();
    //    await using AsyncServiceScope scope2 = _fixture.ServiceProvider.CreateScope();

    //    ISender sender1 = scope1.ServiceProvider.GetRequiredService<ISender>();
    //    ISender sender2 = scope2.ServiceProvider.GetRequiredService<ISender>();

    //    ECommerceDbContext dbContext = scope1.ServiceProvider
    //        .GetRequiredService<ECommerceDbContext>();

    //    string sku = "RACE-SKU-001";

    //    Task<Domain.Common.Result<Guid>> task1 = sender1.Send(CreateValidCommand(
    //        sku: sku,
    //        gtin: "1111111111111"));

    //    Task<Domain.Common.Result<Guid>> task2 = sender2.Send(CreateValidCommand(
    //        sku: sku,
    //        gtin: "2222222222222"));

    //    await Task.WhenAll(task1, task2);

    //    Domain.Common.Result<Guid> result1 = await task1;
    //    Domain.Common.Result<Guid> result2 = await task2;

    //    (result1.IsSuccess ^ result2.IsSuccess).Should().BeTrue();
    //    (result1.IsFailure ^ result2.IsFailure).Should().BeTrue();

    //    List<Product> products = await dbContext.Products
    //        .AsNoTracking()
    //        .Where(x => x.Sku == sku)
    //        .ToListAsync();

    //    products.Should().HaveCount(1);
    //}

    [Fact]
    public async Task CreateProduct_ShouldAllowMultipleProducts_WhenGtinIsNull()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();

        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ECommerceDbContext dbContext = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        int beforeCount = await dbContext.Products.CountAsync();

        CreateProductCommand firstCommand = CreateValidCommand(
            sku: "SKU-200",
            gtin: null);

        CreateProductCommand secondCommand = CreateValidCommand(
            sku: "SKU-201",
            gtin: null);

        Domain.Common.Result<Guid> firstResult = await sender.Send(firstCommand);
        Domain.Common.Result<Guid> secondResult = await sender.Send(secondCommand);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();

        firstResult.Value.Should().NotBeEmpty();
        secondResult.Value.Should().NotBeEmpty();

        int afterCount = await dbContext.Products.CountAsync();
        afterCount.Should().Be(beforeCount + 2);

        Product firstProduct = await dbContext.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == firstResult.Value);

        Product secondProduct = await dbContext.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == secondResult.Value);

        firstProduct.Gtin.Should().BeNull();
        secondProduct.Gtin.Should().BeNull();

        firstProduct.Sku.Should().Be("SKU-200");
        secondProduct.Sku.Should().Be("SKU-201");
    }

    [Fact]
    public async Task CreateProduct_ShouldPersistOnlyOneRow_WhenTwoRequestsUseSameSkuConcurrently()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope1 =
            _fixture.ServiceProvider.CreateScope();

        await using AsyncServiceScope scope2 =
            _fixture.ServiceProvider.CreateScope();

        ISender sender1 =
            scope1.ServiceProvider.GetRequiredService<ISender>();

        ISender sender2 =
            scope2.ServiceProvider.GetRequiredService<ISender>();

        ECommerceDbContext context1 =
            scope1.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        ECommerceDbContext context2 =
            scope2.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        context1.Should().NotBeSameAs(context2);

        SaveChangesTestBarrier barrier =
            scope1.ServiceProvider.GetRequiredService<SaveChangesTestBarrier>();

        const string sku = "RACE-SKU-001";

        CreateProductCommand command1 = CreateValidCommand(
            sku: sku,
            gtin: null);

        CreateProductCommand command2 = CreateValidCommand(
            sku: sku,
            gtin: null);

        barrier.Arm(participantCount: 2);

        Guid createdProductId;

        try
        {
            // Act
            Domain.Common.Result<Guid>[] results = await Task.WhenAll(
                sender1.Send(command1),
                sender2.Send(command2));

            // Assert: exactly one request succeeds.
            results.Should().HaveCount(2);

            results.Count(x => x.IsSuccess).Should().Be(1);
            results.Count(x => x.IsFailure).Should().Be(1);

            Domain.Common.Result<Guid> successfulResult = results.Single(x => x.IsSuccess);
            Domain.Common.Result<Guid> failedResult = results.Single(x => x.IsFailure);

            createdProductId = successfulResult.Value;

            createdProductId.Should().NotBeEmpty();

            failedResult.Error.Code.Should().Be(
                "Products.SkuAlreadyExists");
        }
        finally
        {
            barrier.Disarm();
        }

        // Assert: verify actual PostgreSQL state.
        await using AsyncServiceScope verificationScope =
            _fixture.ServiceProvider.CreateScope();

        ECommerceDbContext verificationContext =
            verificationScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

        List<Product> products = await verificationContext.Products
            .AsNoTracking()
            .Where(x => x.Sku == sku)
            .ToListAsync();

        products.Should().ContainSingle();

        products[0].Id.Should().Be(createdProductId);
        products[0].Sku.Should().Be(sku);
        products[0].Gtin.Should().BeNull();
    }
}
