using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
public record ProductBackInStockEvent(
    Guid ProductId) : DomainEvent;
