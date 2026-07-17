using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    public record ProductDeletedEvent(Guid ProductId) : DomainEvent;
}
