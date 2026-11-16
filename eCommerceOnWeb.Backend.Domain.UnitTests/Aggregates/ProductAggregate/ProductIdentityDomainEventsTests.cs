using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;
public class ProductIdentityDomainEventsTests
{
    [Fact]
    public void IdentityFieldChanges_ShouldNotRaiseDomainEvents()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // ProductTestData очищает DomainEvents после Arrange.

        // Act
        product.Rename("New Smartphone");
        product.ChangeSku("NEW-SKU");
        product.ChangeGtin("9876543210123");
        product.ChangeModelNumber("NEW-MODEL");
        product.ChangeCategory(Guid.NewGuid());
        product.ChangeBrand(Guid.NewGuid());

        // Assert
        product.DomainEvents.Should().BeEmpty();
    }
}

