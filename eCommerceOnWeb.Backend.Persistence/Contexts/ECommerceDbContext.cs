using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common; // ISoftDeletable лежит здесь
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace eCommerceOnWeb.Backend.Persistence.Contexts;

public class ECommerceDbContext : DbContext, IECommerceDbContext
{
    private readonly IMediator _mediator;
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options,
        IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Глобально запрещаем маппить базовый доменный класс событий
        builder.Ignore<DomainEvent>();

        // Автоматически применяем конфигурации из вашей папки EntityTypeConfigurations
        builder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        throw new NotSupportedException(
            "Use SaveChangesAsync() instead.");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new NotSupportedException(
            "Use SaveChangesAsync() instead.");
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(
            acceptAllChangesOnSuccess: true,
            cancellationToken: cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();

        IReadOnlyCollection<DomainEvent> domainEvents = GetDomainEvents();

        int result = await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);

        try
        {
            await PublishDomainEvents(domainEvents, cancellationToken);
        }
        finally
        {
            ClearDomainEvents();
        }

        return result;
    }

    private void ApplySoftDelete()
    {
        List<EntityEntry<ISoftDeletable>> entries = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (EntityEntry<ISoftDeletable> entry in entries)
        {
            entry.State = EntityState.Modified;

            if (!entry.Entity.IsDeleted)
            {
                entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;

                entry.CurrentValues[nameof(ISoftDeletable.DeletedAtUtc)] =
                    DateTime.UtcNow;
            }

            // Обновляем только поля soft delete.
            foreach (PropertyEntry property in entry.Properties)
            {
                property.IsModified =
                    property.Metadata.Name == nameof(ISoftDeletable.IsDeleted) ||
                    property.Metadata.Name == nameof(ISoftDeletable.DeletedAtUtc);
            }
        }
    }

    private IReadOnlyCollection<DomainEvent> GetDomainEvents()
    {
        return ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Any())
            .SelectMany(entity => entity.DomainEvents)
            .ToList();
    }

    private void ClearDomainEvents()
    {
        IEnumerable<BaseEntity> entities = ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Any());

        foreach (BaseEntity entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }

    private async Task PublishDomainEvents(
    IReadOnlyCollection<DomainEvent> domainEvents,
    CancellationToken cancellationToken)
    {
        foreach (DomainEvent domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}