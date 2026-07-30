using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common; // ISoftDeletable лежит здесь
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace eCommerceOnWeb.Backend.Persistence.Contexts;

public class ECommerceDbContext : DbContext, IECommerceDbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>(); //cs0103

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); //cs0117

        // Глобально запрещаем маппить базовый доменный класс событий
        builder.Ignore<DomainEvent>();

        // Автоматически применяем конфигурации из вашей папки EntityTypeConfigurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // Перехват сохранения изменений для автоматического Soft Delete
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    private void ApplySoftDelete()
    {
        // Находим все сущности в состоянии Deleted, которые реализуют ISoftDeletable
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ISoftDeletable>> entries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ISoftDeletable>? entry in entries)
        {
            // Отменяем физическое удаление из базы данных
            entry.State = EntityState.Modified;

            // Заполняем доменные свойства мягкого удаления
            entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;
            entry.CurrentValues[nameof(ISoftDeletable.DeletedAtUtc)] = DateTime.UtcNow;

            // Если у сущности есть свойство IsActive (как у Product), его тоже выключаем [3]
            if (entry.CurrentValues.Properties.Any(p => p.Name == "IsActive"))
            {
                entry.CurrentValues["IsActive"] = false;
            }
        }
    }
}