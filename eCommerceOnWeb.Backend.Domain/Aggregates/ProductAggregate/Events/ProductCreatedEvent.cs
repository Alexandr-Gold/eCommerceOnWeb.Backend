using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    public record ProductCreatedEvent(
        Guid ProductId,
        string Sku,
        Money Price) : DomainEvent;
}
