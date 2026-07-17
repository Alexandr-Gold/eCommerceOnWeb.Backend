using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    // Кратко, неизменяемо и чисто:
    public record ProductPriceChangedEvent(
        Guid ProductId,
        Money OldPrice,
        Money NewPrice) : DomainEvent;
}
