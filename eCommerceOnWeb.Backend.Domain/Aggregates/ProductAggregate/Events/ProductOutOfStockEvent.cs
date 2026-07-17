using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    public record ProductOutOfStockEvent(Guid ProductId) : DomainEvent;
}
