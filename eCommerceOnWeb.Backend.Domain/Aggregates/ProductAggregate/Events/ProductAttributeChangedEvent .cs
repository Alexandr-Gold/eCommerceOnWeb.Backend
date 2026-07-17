using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events
{
    public sealed record ProductAttributeChangedEvent : DomainEvent
    {
        public Guid ProductId { get; init; }
        public string Key { get; init; }
        public object OldValue { get; init; }
        public object NewValue { get; init; }

        public ProductAttributeChangedEvent(Guid productId, string key, object oldValue, object newValue)
        {
            ProductId = productId;
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
