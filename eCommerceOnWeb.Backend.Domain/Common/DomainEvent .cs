using MediatR;
using System.Text.Json.Serialization;

namespace eCommerceOnWeb.Backend.Domain.Common
{

    public abstract record DomainEvent : INotification
    {
        public Guid EventId { get; }                // только get – защита от with
        public DateTime OccurredOnUtc { get; }

        // Конструктор для обычного создания (используется производными записями)
        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
        }

        // Конструктор для десериализации (System.Text.Json)
        [JsonConstructor]
        protected DomainEvent(Guid eventId, DateTime occurredOnUtc)
        {
            EventId = eventId;
            OccurredOnUtc = occurredOnUtc;
        }
    }


    //public abstract record DomainEvent : INotification
    //{
    //    public Guid EventId { get; init; } = Guid.NewGuid();
    //    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    //}
}
