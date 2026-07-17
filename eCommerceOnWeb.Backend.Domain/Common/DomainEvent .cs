using MediatR;

namespace eCommerceOnWeb.Backend.Domain.Common
{
    public abstract record DomainEvent : INotification
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    }
}
