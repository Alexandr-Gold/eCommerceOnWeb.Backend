using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;
public class ProductStatusTests
{
    [Theory]
    [InlineData(ProductStatus.Draft, ProductStatus.Active)]
    [InlineData(ProductStatus.Active, ProductStatus.Inactive)]
    [InlineData(ProductStatus.Inactive, ProductStatus.Draft)]
    public void DistinctValues_ShouldNotBeEqual(ProductStatus a, ProductStatus b)
    {
        a.Should().NotBe(b);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.Active)]
    [InlineData(ProductStatus.Inactive)]
    [InlineData(ProductStatus.Discontinued)]
    public void SameValue_ShouldBeEqual(ProductStatus value)
    {
        value.Should().Be(value);
    }

    [Fact]
    public void Debug_StatusMismatch()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 10);

        ProductStatus status = product.Status;
        ProductStatus active = ProductStatus.Active;

        Console.WriteLine($"Status: value={status} int={(int)status} type={status.GetType().FullName} asm={status.GetType().Assembly.FullName}");
        Console.WriteLine($"Active: value={active} int={(int)active} type={active.GetType().FullName} asm={active.GetType().Assembly.FullName}");
        Console.WriteLine($"== : {status == active}");
        Console.WriteLine($"is : {status is ProductStatus.Active}");
        Console.WriteLine($"is not : {status is not ProductStatus.Active}");
        Console.WriteLine($"Equals: {status.Equals(active)}");
        Console.WriteLine($"Product type: {product.GetType().FullName}");
        Console.WriteLine($"Product asm: {product.GetType().Assembly.FullName}");
    }
}
