using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    public record ProductDiscontinuedEvent(
        Guid ProductId,
        string? Reason) : DomainEvent;
}
