using eCommerceOnWeb.Backend.Domain.Entities;
using eCommerceOnWeb.Backend.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eCommerceOnWeb.Backend.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // РЕГИСТРАЦИЯ ТАБЛИЦ КАТАЛОГА
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое сканирование всех сущностей домена
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Изолируем таблицы в схему 'app' по Паспорту Проекта
                modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name, "app");

                // --- ИСПРАВЛЕНИЕ ОШИБКИ CS8917 ЧЕРЕЗ EXPRESSION TREES ---
                // Генерируем лямбду: e => EF.Property<bool>(e, "IsDeleted") == false
                ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
                System.Reflection.MethodInfo propertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool));
                MethodCallExpression isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant(nameof(BaseEntity.IsDeleted)));
                BinaryExpression compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                LambdaExpression lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                // ---------------------------------------------------------

                // Оптимизация: B-Tree индекс на флаг удаления для PostgreSQL
                modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(BaseEntity.IsDeleted));
            }
        }

        // ЯВНОЕ ОПИСАНИЕ СВЯЗЕЙ (FLUENT API) С ЗАЩИТОЙ ОТ КАСКАДНОГО УДАЛЕНИЯ
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить категорию, если в ней есть товары

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить бренд, если к нему привязаны товары

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Защита для иерархии категорий
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Временная Guid-заглушка согласно Паспорту проекта
        string currentUserId = "00000000-0000-0000-0000-000000000001";

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(x => x.CreatedBy).CurrentValue = currentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.MarkAsUpdated(currentUserId);
                    break;

                case EntityState.Deleted:
                    // Перехватываем физическое удаление и превращаем в Soft Delete
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(currentUserId);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}