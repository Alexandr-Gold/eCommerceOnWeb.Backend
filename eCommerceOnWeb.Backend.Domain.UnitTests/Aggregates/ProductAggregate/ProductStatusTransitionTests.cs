using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;

public class ProductStatusTransitionTests
{
    [Fact]
    public void Activate_ShouldChangeStatus_FromDraftToActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Status.Should()
            .Be(ProductStatus.Draft);

        // Act
        product.Activate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_ShouldDoNothing_WhenAlreadyActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Activate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Activate_ShouldChangeStatus_FromInactiveToActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        product.Status.Should()
            .Be(ProductStatus.Inactive);

        // Act
        product.Activate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_ShouldThrow_WhenProductIsDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        // Act
        Action act = () =>
            product.Activate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Activate_ShouldThrow_WhenDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        product.ClearDomainEvents();

        // Act
        Action act = () => product.Activate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .BeEmpty();
    }

    // ----------------------------------------------------
    // Deactivate
    // ----------------------------------------------------

    [Fact]
    public void Deactivate_ShouldChangeStatus_FromActiveToInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        // Act
        product.Deactivate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Inactive);
    }

    [Fact]
    public void Deactivate_ShouldDoNothing_WhenAlreadyInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Deactivate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Inactive);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenProductIsDraft()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        // Act
        Action act = () =>
            product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Draft);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenProductIsDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        // Act
        Action act = () =>
            product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenDraft()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        // Act
        Action act = () => product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Draft);

        product.DomainEvents.Should()
            .BeEmpty();
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        product.ClearDomainEvents();

        // Act
        Action act = () => product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .BeEmpty();
    }

    // ----------------------------------------------------
    // Discontinue
    // ----------------------------------------------------

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromDraftToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        // Act
        product.Discontinue();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromActiveToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        // Act
        product.Discontinue();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromInactiveToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        // Act
        product.Discontinue();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Discontinue_ShouldDoNothing_WhenAlreadyDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        int eventsCount =
            product.DomainEvents.Count;


        // Act
        product.Discontinue();


        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }
}
