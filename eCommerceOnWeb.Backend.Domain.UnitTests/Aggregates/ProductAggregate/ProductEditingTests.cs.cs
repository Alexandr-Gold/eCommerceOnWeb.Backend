using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;
public class ProductEditingTests
{
    [Fact]
    public void Draft_Product_ShouldAllow_Rename()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft);

        // Act
        product.Rename("New Smartphone");

        // Assert
        product.Name.Should().Be("New Smartphone");
        product.Status.Should().Be(ProductStatus.Draft);
    }


    [Fact]
    public void Active_Product_ShouldNotAllow_Rename()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active);

        // Act
        Action act = () =>
            product.Rename("New Smartphone");

        // Assert
        act.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Draft_Product_ShouldAllow_ChangeSku()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft);

        // Act
        product.ChangeSku("new-sku");

        // Assert
        product.Sku.Should().Be("NEW-SKU");
    }


    [Fact]
    public void Active_Product_ShouldNotAllow_ChangeSku()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active);

        // Act
        Action act = () =>
            product.ChangeSku("new-sku");

        // Assert
        act.Should()
            .Throw<DomainException>();
    }


    [Fact]
    public void Editing_Active_Product_ShouldKeep_Status()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active);

        // Act
        product.UpdateDescription(
            "Updated description");

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);
    }
}