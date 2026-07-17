namespace eCommerceOnWeb.Backend.Domain.Common
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; }
        DateTime? DeletedAtUtc { get; }
        void Delete(); // Метод для вызова внутри домена
    }
}
