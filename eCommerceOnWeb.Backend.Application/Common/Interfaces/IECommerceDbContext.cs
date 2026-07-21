using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Application.Common.Interfaces
{
    public interface IECommerceDbContext
    {
        // --- 1. Доступ к таблицам (Коллекциям сущностей) ---
        DbSet<Product> Products { get; }

        // Сюда же со временем добавятся остальные сущности:
        // DbSet<Category> Categories { get; }
        // DbSet<Brand> Brands { get; }
        // DbSet<Order> Orders { get; }

        // --- 2. Управление транзакциями и сохранением (Единица работы / Unit of Work) ---

        // Самый главный метод для сохранения изменений команд (Queries)
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // --- 3. Методы низкоуровневого управления сущностями (Для сложных сценариев) ---

        // Позволяет вручную управлять состояниями объектов (например, при отслеживании)
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry(object entity);
    }
}
