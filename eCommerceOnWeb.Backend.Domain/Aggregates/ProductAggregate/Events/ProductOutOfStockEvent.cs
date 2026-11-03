using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    // ProductOutOfStockEvent относится к складскому состоянию.
    public record ProductOutOfStockEvent(
        Guid ProductId) : DomainEvent;
}
