using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common; // ISoftDeletable лежит здесь
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace eCommerceOnWeb.Backend.Persistence.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

    /// <summary>
    /// Динамически генерирует лямбда-выражение (e => !e.IsDeleted) для фильтра EF Core.
    /// </summary>
    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
    {
        System.Linq.Expressions.ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        System.Linq.Expressions.MemberExpression property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        System.Linq.Expressions.UnaryExpression notExpression = System.Linq.Expressions.Expression.Not(property);
        return System.Linq.Expressions.Expression.Lambda(notExpression, parameter);
    }

    private static string ConvertToSnakeCase(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return System.Text.RegularExpressions.Regex
            .Replace(input, "([a-z0-9])([A-Z])", "$1_$2")
            .ToLowerInvariant();
    }
}