namespace eCommerceOnWeb.Backend.Domain.Common
{
    /// <summary>
    /// Базовый класс для всех сущностей с поддержкой аудита и Soft Delete.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        // Скрытая коллекция для хранения событий внутри сущности
        private readonly List<DomainEvent> _domainEvents = new();

        // Публичное свойство только для чтения, чтобы Infrastructure могла забрать события перед сохранением
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // МЕТОД позволяет сущностям регистрировать события
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        // Метод очистки событий после того, как они были сохранены или опубликованы
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }
    }
}
