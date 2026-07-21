using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Persistence.Contexts.EntityTypeConfigurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        // Первичный ключ
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();
        builder.Ignore(p => p.DomainEvents);

        // --- Базовые свойства ---
        builder.Property(p => p.Name)
               .HasColumnName("name")
               .HasMaxLength(250)
               .IsRequired();

        builder.Property(p => p.Sku)
               .HasColumnName("sku")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.ModelNumber)
               .HasColumnName("model_number")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.Gtin)
               .HasColumnName("gtin")
               .HasMaxLength(13)
               .IsFixedLength(true);

        builder.Property(p => p.WarrantyMonths)
               .HasColumnName("warranty_months")
               .IsRequired();

        builder.Property(p => p.StockQuantity)
               .HasColumnName("stock_quantity")
               .IsRequired();

        builder.Property(p => p.IsActive)
               .HasColumnName("is_active")
               .IsRequired();

        // --- Индексы ---
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Gtin).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);

        // 1. Сначала полностью конфигурируем свойство в БД
        builder.Property(p => p.CategoryId)
               .HasColumnName("category_id")
               .IsRequired();

        builder.Property(p => p.BrandId)
               .HasColumnName("brand_id")
               .IsRequired();

        // --- Soft Delete ---
        builder.Property(p => p.IsDeleted)
               .HasColumnName("is_deleted")
               .IsRequired();

        builder.Property(p => p.DeletedAtUtc)
               .HasColumnName("deleted_at_utc");

        // 2. И только потом применяем фильтр, когда EF Core точно знает имя столбца
        builder.HasQueryFilter(p => !p.IsDeleted);

        // --- Value Object: Price ---
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                 .HasColumnName("price_amount")
                 .HasPrecision(18, 2)
                 .IsRequired();

            price.Property(m => m.Currency)
                 .HasColumnName("price_currency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        // --- Value Object: ShippingDimensions ---
        builder.OwnsOne(p => p.ShippingDimensions, dim =>
        {
            dim.Property(d => d.WidthCm)
               .HasColumnName("dim_width_cm")
               .HasPrecision(10, 2)
               .IsRequired();

            dim.Property(d => d.HeightCm)
               .HasColumnName("dim_height_cm")
               .HasPrecision(10, 2)
               .IsRequired();

            dim.Property(d => d.DepthCm)
               .HasColumnName("dim_depth_cm")
               .HasPrecision(10, 2)
               .IsRequired();

            dim.Property(d => d.WeightKg)
               .HasColumnName("dim_weight_kg")
               .HasPrecision(10, 2)
               .IsRequired();
        });

        // --- Дочерняя коллекция: Images ---
        builder.OwnsMany(p => p.Images, img =>
        {
            img.ToTable("product_images");
            img.Ignore(i => i.DomainEvents);

            img.HasKey(i => i.Id);
            img.Property(i => i.Id)
               .HasColumnName("id")
               .ValueGeneratedNever();

            img.Property(i => i.StorageKey)
               .HasColumnName("storage_key")
               .HasMaxLength(1000)
               .IsRequired();

            img.Property(i => i.AltText)
               .HasColumnName("alt_text")
               .HasMaxLength(250)
               .IsRequired();

            img.Property(i => i.DisplayOrder)
               .HasColumnName("display_order")
               .IsRequired();

            img.Property(i => i.IsMain)
               .HasColumnName("is_main")
               .IsRequired();

            // Внешний ключ к Products
            img.WithOwner()
               .HasForeignKey("product_id");
        });

        builder.Navigation(p => p.Images)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // --- Конфигурация JSONB для ProductAttributes ---
        builder.Property(p => p.Attributes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<ProductAttributes>(v, new JsonSerializerOptions()) ?? ProductAttributes.Empty()
            )
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        // GIN-индекс для быстрого поиска по атрибутам
        builder.HasIndex(p => p.Attributes)
            .HasMethod("GIN")
            .HasDatabaseName("IX_products_attributes_gin");

        // (Опционально) Индекс на конкретный путь внутри JSON
        // builder.HasIndex(p => NpgsqlDbFunctionsExtensions.JsonValue(EF.Functions, p.Attributes, "$.Color"))
        //     .HasDatabaseName("IX_products_color");
    }
}