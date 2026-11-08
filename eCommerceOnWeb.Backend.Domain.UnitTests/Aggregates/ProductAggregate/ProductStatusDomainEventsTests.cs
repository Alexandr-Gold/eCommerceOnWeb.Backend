using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;

public class ProductStatusDomainEventsTests
{
    [Fact]
    public void Activate_ShouldRaiseProductActivatedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();
        product.Status.Should().Be(ProductStatus.Draft); // precondition

        // Act
        product.Activate();

        // Assert
        product.DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<ProductActivatedEvent>();
    }

    [Fact]
    public void Activate_ShouldNotRaiseEvent_WhenAlreadyActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        // Act
        product.Activate();
        product.ClearDomainEvents();

        // Assert
        product.DomainEvents.Should()
            .BeEmpty();
    }

    [Fact]
    public void Activate_FromInactive_ShouldRaiseProductActivatedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        product.Status.Should()
            .Be(ProductStatus.Inactive);

        product.ClearDomainEvents();

        // Act
        product.Activate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);

        ProductActivatedEvent domainEvent =
            product.DomainEvents
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeOfType<ProductActivatedEvent>()
                .Subject;

        domainEvent.ProductId.Should()
            .Be(product.Id);
    }

    [Fact]
    public void Deactivate_ShouldRaiseProductDeactivatedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();             // готовим Active
        product.ClearDomainEvents();    // ← очищаем ПОСЛЕ подготовки

        // Act
        product.Deactivate();

        // Assert
        product.DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<ProductDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_ShouldNotRaiseEvent_WhenAlreadyInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();
        product.ClearDomainEvents();

        // Act
        product.Deactivate();


        // Assert
        product.DomainEvents.Should()
            .BeEmpty();
    }

    [Fact]
    public void Discontinue_ShouldRaiseProductDiscontinuedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        // Act
        product.Discontinue();

        // Assert
        ProductDiscontinuedEvent domainEvent =
            product.DomainEvents
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeOfType<ProductDiscontinuedEvent>()
                .Subject;

        domainEvent.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void Discontinue_ShouldNotRaiseEvent_WhenAlreadyDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();
        product.ClearDomainEvents();

        // Act
        product.Discontinue();

        // Assert
        product.DomainEvents.Should()
            .BeEmpty();
    }

    [Fact]
    public void Discontinue_ShouldPropagateReasonToEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        const string reason = "End of life";

        // Act
        product.Discontinue(reason);

        // Assert
        ProductDiscontinuedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductDiscontinuedEvent>()
                .Single();

        domainEvent.ProductId.Should()
            .Be(product.Id);

        domainEvent.Reason.Should()
            .Be(reason);
    }

    [Fact]
    public void Discontinue_WithoutReason_ShouldHaveNullReason()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        // Act
        product.Discontinue();

        // Assert
        ProductDiscontinuedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductDiscontinuedEvent>()
                .Single();

        domainEvent.ProductId.Should()
            .Be(product.Id);

        domainEvent.Reason.Should()
            .BeNull();
    }
}

