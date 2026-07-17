using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Persistence.Data.EntityTypeConfigurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Ignore(p => p.DomainEvents);

            // --- Базовые свойства ---
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
            builder.Property(p => p.ModelNumber).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Gtin).HasMaxLength(13).IsFixedLength(true);
            builder.Property(p => p.WarrantyMonths).IsRequired();
            builder.Property(p => p.StockQuantity).IsRequired();
            builder.Property(p => p.IsActive).IsRequired();

            // --- Индексы ---
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.Gtin).IsUnique();
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BrandId);

            // --- Soft Delete ---
            builder.HasQueryFilter(p => !p.IsDeleted);
            builder.Property(p => p.DeletedAtUtc);

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
                dim.Property(d => d.WidthCm).HasColumnName("dim_width_cm").HasPrecision(10, 2).IsRequired();
                dim.Property(d => d.HeightCm).HasColumnName("dim_height_cm").HasPrecision(10, 2).IsRequired();
                dim.Property(d => d.DepthCm).HasColumnName("dim_depth_cm").HasPrecision(10, 2).IsRequired();
                dim.Property(d => d.WeightKg).HasColumnName("dim_weight_kg").HasPrecision(10, 2).IsRequired();
            });

            // --- Дочерняя коллекция: Images (без изменений) ---
            builder.OwnsMany(p => p.Images, img =>
            {
                img.ToTable("product_images");
                img.Ignore(i => i.DomainEvents);
                img.HasKey(i => i.Id);
                img.Property(i => i.Id).ValueGeneratedNever();

                img.Property(i => i.Url).HasMaxLength(1000).IsRequired();
                img.Property(i => i.AltText).HasMaxLength(250).IsRequired();
                img.Property(i => i.DisplayOrder).IsRequired();
                img.Property(i => i.IsMain).IsRequired();

                img.WithOwner().HasForeignKey("ProductId");
            });

            builder.Navigation(p => p.Images)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            // --- НОВОЕ: Конфигурация JSONB для ProductAttributes через Value Converter ---
            builder.Property(p => p.Attributes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<ProductAttributes>(v, new JsonSerializerOptions()) ?? ProductAttributes.Empty()
                )
                .HasColumnName("attributes")
                .HasColumnType("jsonb");

            // GIN-индекс для быстрого поиска по атрибутам внутри JSON
            builder.HasIndex(p => p.Attributes)
                .HasMethod("GIN")
                .HasDatabaseName("IX_products_attributes_gin");

            // GIN-индекс для быстрого поиска по атрибутам внутри JSON
            builder.HasIndex(p => p.Attributes)
                   .HasMethod("GIN")
                   .HasDatabaseName("IX_products_attributes_gin");

            // При необходимости можно добавить индексы на конкретные пути внутри JSON
            // builder.HasIndex(p => EF.Functions.JsonValue(p.Attributes, "$.Color"))
            //        .HasDatabaseName("IX_products_color");
        }
    }
}




//------------------------------------------------------

//using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace eCommerceOnWeb.Backend.Persistence.Data.EntityTypeConfigurations
//{
//    // <summary>
//    /// Конфигурация Fluent API для корня агрегата Product и его внутренних компонентов.
//    /// Полностью изолирует инфраструктуру от бизнес-логики домена.
//    /// </summary>
//    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
//    {
//        public void Configure(EntityTypeBuilder<Product> builder)
//        {

//            // 1. Первичный ключ и базовая таблица
//            builder.ToTable("products"); // Имя таблицы (плагин переведет в snake_case автоматически)
//            builder.HasKey(p => p.Id);
//            builder.Property(p => p.Id)
//                   .ValueGeneratedNever(); // Идентификаторы Guid генерируются на уровне Домена
//            builder.Ignore(p => p.DomainEvents);
//            // 2. Строгие ограничения полей (Специфика Электроники)
//            builder.Property(p => p.Name)
//                   .HasMaxLength(250)
//                   .IsRequired();

//            builder.Property(p => p.Sku)
//                   .HasMaxLength(100)
//                   .IsRequired();

//            builder.Property(p => p.ModelNumber)
//                   .HasMaxLength(100)
//                   .IsRequired(); // Метод ValidateBasicAttributes требует обязательного заполнения

//            builder.Property(p => p.Gtin)
//                   .HasMaxLength(13)
//                   .IsFixedLength(true); // EAN-13 имеет фиксированную длину 13 символов

//            builder.Property(p => p.WarrantyMonths)
//                   .IsRequired();

//            builder.Property(p => p.StockQuantity)
//                   .IsRequired();

//            builder.Property(p => p.IsActive)
//                   .IsRequired();

//            // 3. Индексы уникальности и быстрого поиска
//            builder.HasIndex(p => p.Sku).IsUnique(); // Защита от дубликатов артикулов на уровне БД
//            builder.HasIndex(p => p.Gtin).IsUnique(); // Защита от дубликатов штрих-кодов

//            // Навешиваем индексы на внешние Guid-ссылки для быстрого связывания (Join-запросов)
//            builder.HasIndex(p => p.CategoryId);
//            builder.HasIndex(p => p.BrandId);

//            // 4. Глобальный фильтр для Мягкого Удаления (Soft Delete)
//            builder.HasQueryFilter(p => !p.IsDeleted);
//            builder.Property(p => p.DeletedAtUtc);

//            // 5. Маппинг Value Object: Price (Money)
//            builder.OwnsOne(p => p.Price, price =>
//            {
//                price.Property(m => m.Amount)
//                     .HasColumnName("price_amount") // Явное имя для ясности в Postgres
//                     .HasPrecision(18, 2)           // Высокая точность для денежных расчетов
//                     .IsRequired();

//                price.Property(m => m.Currency)
//                     .HasColumnName("price_currency")
//                     .HasMaxLength(3)               // ISO-коды валют всегда состоят из 3 букв (RUB, USD)
//                     .IsRequired();
//            });

//            // 6. Маппинг Value Object: ShippingDimensions (Dimensions)
//            builder.OwnsOne(p => p.ShippingDimensions, dim =>
//            {
//                // Используем точные имена свойств из вашего доменного record Dimensions
//                dim.Property(d => d.WidthCm)
//                   .HasColumnName("dim_width_cm") // Красивое имя колонки для PostgreSQL
//                   .HasPrecision(10, 2)
//                   .IsRequired();

//                dim.Property(d => d.HeightCm)
//                   .HasColumnName("dim_height_cm")
//                   .HasPrecision(10, 2)
//                   .IsRequired();

//                dim.Property(d => d.DepthCm)
//                   .HasColumnName("dim_depth_cm")
//                   .HasPrecision(10, 2)
//                   .IsRequired();

//                dim.Property(d => d.WeightKg)
//                   .HasColumnName("dim_weight_kg")
//                   .HasPrecision(10, 2)
//                   .IsRequired();
//            });

//            // 7. Инкапсуляция дочерней коллекции: Images (_images)
//            builder.OwnsMany(p => p.Images, img =>
//            {
//                img.ToTable("product_images");
//                img.Ignore(i => i.DomainEvents);
//                img.HasKey(i => i.Id);
//                img.Property(i => i.Id).ValueGeneratedNever();

//                img.Property(i => i.Url).HasMaxLength(1000).IsRequired();
//                img.Property(i => i.AltText).HasMaxLength(250).IsRequired();
//                img.Property(i => i.DisplayOrder).IsRequired();
//                img.Property(i => i.IsMain).IsRequired();

//                // Создаем теневое свойство (Shadow Property) Внешнего Ключа на таблицу Products
//                img.WithOwner().HasForeignKey("ProductId");
//            });

//            // Говорим EF Core загружать/читать данные в обход публичного IReadOnlyCollection свойства,
//            // используя приватное поле _images. Это сохраняет инкапсуляцию домена.
//            builder.Navigation(p => p.Images)
//                   .UsePropertyAccessMode(PropertyAccessMode.Field);

//            // 8. Инкапсуляция дочерней коллекции: AttributeValues (_attributeValues)
//            builder.OwnsMany(p => p.AttributeValues, attr =>
//            {
//                attr.ToTable("product_attribute_values");
//                attr.Ignore(a => a.DomainEvents);

//                // Задаем первичный ключ для таблицы значений атрибутов
//                attr.HasKey(a => a.Id);
//                attr.Property(a => a.Id)
//                    .ValueGeneratedNever();

//                // Ссылка на глобальный справочник характеристик
//                attr.Property(a => a.AttributeId)
//                    .IsRequired();

//                // Денормализованное имя характеристики (например, "Цвет")
//                attr.Property(a => a.AttributeName)
//                    .HasMaxLength(150)
//                    .IsRequired();

//                // Строковое представление значения для экрана (например, "Красный")
//                attr.Property(a => a.DisplayValue)
//                    .HasMaxLength(500)
//                    .IsRequired();

//                // Числовое значение для точной фильтрации (например, объем памяти или диагональ)
//                attr.Property(a => a.NumericValue)
//                    .HasPrecision(18, 4) // Высокая точность для любых технических параметров
//                    .IsRequired(false);  // Поле может быть null согласно домену

//                // Логическое значение для фильтров-флагов (например, "Влагозащита: Да/Нет")
//                attr.Property(a => a.BooleanValue)
//                    .IsRequired(false);  // Поле может быть null согласно домену

//                // Настройка внешнего ключа в стиле snake_case для PostgreSQL
//                attr.WithOwner().HasForeignKey("product_id");

//                // Индексы для быстрой фильтрации товаров в каталоге
//                attr.HasIndex(a => a.AttributeId);
//                attr.HasIndex(a => new { a.AttributeId, a.NumericValue })
//                    .HasFilter("numeric_value IS NOT NULL"); // Частичный индекс для числовых фильтров
//            });

//            // Настройка доступа к приватному полю коллекции EAV-атрибутов
//            builder.Navigation(p => p.AttributeValues)
//                   .UsePropertyAccessMode(PropertyAccessMode.Field);
//        }
//    }
//}
