using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{

    //    Create()
    //AddImage()
    //SetAttribute()
    //ChangePrice()
    //DeductStock()
    //Restock()
    //Delete()

    //Нельзя изменять свойства напрямую из Application слоя.

    //Product публикует Domain Events:
    //- ProductCreatedEvent
    //- ProductPriceChangedEvent
    //- ProductAttributeChangedEvent
    //- ProductDeletedEvent

    //Текущая публикация Domain Events выполняется через DbContext.
    //Перед критическими сценариями (Cart / Order / Payment)
    //перейти на:
    //EF Core SaveChangesInterceptor + Outbox Pattern.

    public class Product : BaseEntity, IAggregateRoot, ISoftDeletable
    {
        // --- Базовые свойства ---
        public string Name { get; private set; } = string.Empty;
        public string Sku { get; private set; } = string.Empty; // Внутренний артикул (например, "IPH-15PRO-128")
        public string? Gtin { get; private set; } // Международный штрих-код (EAN-13 / UPC) для сканеров и маркетплейсов
        public string ModelNumber { get; private set; } = string.Empty; // Номер модели производителя (например, "A3106")
        public Money Price { get; private set; } = Money.Zero();

        // --- Специфика электроники ---
        public int WarrantyMonths { get; private set; } // Гарантия в месяцах (0 - без гарантии)
        public Dimensions ShippingDimensions { get; private set; } = null!;
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; } = true;

        // --- Связи по ID ---
        public Guid CategoryId { get; private set; }
        public Guid BrandId { get; private set; }

        public string Description { get; private set; } = string.Empty;

        // --- Инкапсулированные коллекции (DDD-стиль) ---
        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images =>
            _images.AsReadOnly();

        public ProductAttributes Attributes { get; private set; }
            = ProductAttributes.Empty();

        // --- Софт-делит ---
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        private Product() { } // Для EF Core

        public Product(
            string name,
            string sku,
            string? gtin,
            string modelNumber,
            Money price,
            Guid categoryId,
            Guid brandId,
            string description,
            int warrantyMonths,
            Dimensions shippingDimensions,
            int initialStock = 0)
        {
            ValidateBasicAttributes(name, sku, modelNumber, categoryId,
                brandId);

            if (warrantyMonths < 0) throw new DomainException(
                "Гарантия не может быть отрицательной.");

            if (initialStock < 0)
                throw new DomainException(
                    "Начальный запас не может быть отрицательным.");

            Name = name.Trim();
            Sku = sku.Trim().ToUpperInvariant();
            Gtin = string.IsNullOrWhiteSpace(gtin)
                 ? null
                 : gtin.Trim();
            ModelNumber = modelNumber.Trim();
            Price = price ?? throw new ArgumentNullException(nameof(price));
            CategoryId = categoryId;
            BrandId = brandId;
            Description = description?.Trim() ?? string.Empty;
            WarrantyMonths = warrantyMonths;
            ShippingDimensions = shippingDimensions ?? throw new ArgumentNullException(nameof(shippingDimensions));
            StockQuantity = initialStock;
        }

        // --- МЕТОДЫ УПРАВЛЕНИЯ ИЗОБРАЖЕНИЯМИ (Инварианты галереи) ---

        public void AddImage(
            string storageKey,
            string altText,
            int displayOrder,
            bool setAsMain = false)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new DomainException("Storage Key изображения обязателен.");

            // Нормализуем ключ до любых проверок, чтобы отсечь скрытые дубликаты с пробелами
            string normalizedStorageKey = storageKey.Trim();

            if (_images.Any(i => i.StorageKey.Equals(normalizedStorageKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException("Такое изображение уже добавлено к товару.");
            }

            // Передаем уже очищенный ключ в конструктор сущности
            ProductImage image = new ProductImage(
                normalizedStorageKey,
                altText,
                displayOrder);

            _images.Add(image);

            SortImages();
            //_images.Sort((x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder)); // Хранит коллекцию всегда отсортированной в памяти

            // Если передан флаг setAsMain или это вообще первая картинка товара, делаем её главной
            if (setAsMain || _images.Count == 1)
            {
                SetMainImage(image.Id);
            }
        }

        public void SetMainImage(Guid imageId)
        {
            ProductImage? targetImage = _images.FirstOrDefault(
                i => i.Id == imageId);

            if (targetImage == null) throw new DomainException(
                "Изображение не найдено в галерее товара.");

            // Сбрасываем флаг главного у всех и ставим выбранному [1, 2]
            foreach (ProductImage img in _images)
            {
                img.IsMain = false;
            }
            targetImage.IsMain = true;
        }

        public void RemoveImage(Guid imageId)
        {
            ProductImage? image = _images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) return;

            _images.Remove(image);

            SortImages();
            //_images.Sort((x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder)); // Хранит коллекцию всегда отсортированной в памяти

            if (image.IsMain && _images.Any())
            {
                ProductImage newMainImage = _images.First();

                newMainImage.IsMain = true;
            }
        }

        // --- ДРУГИЕ БИЗНЕС-МЕТОДЫ ---

        public void UpdateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException(
                    "Описание товара не может быть пустым.");

            string normalizedDescription = description.Trim();

            if (Description.Equals(
                    normalizedDescription,
                    StringComparison.Ordinal))

                return;

            Description = normalizedDescription;
        }

        // Установить или обновить атрибут
        public void SetAttribute(string key, object value)
        {
            object? oldValue = GetAttribute<object>(key);
            Attributes = Attributes.SetValue(key, value);
            AddDomainEvent(
                new ProductAttributeChangedEvent(
                Id,
                key,
                oldValue,
                value));
        }

        // Удалить атрибут
        public void RemoveAttribute(string key)
        {
            object? oldValue = GetAttribute<object>(key);

            if (oldValue is null)
                return;

            Attributes = Attributes.RemoveValue(key);

            AddDomainEvent(
                new ProductAttributeChangedEvent(
                    Id,
                    key,
                    oldValue,
                    null));
        }

        // Получить значение атрибута (для внутренних нужд)
        public T? GetAttribute<T>(string key) => Attributes.GetValue<T>(key);

        public void UpdateShippingDimensions(Dimensions newDimensions)
        {
            ArgumentNullException.ThrowIfNull(newDimensions);

            if (ShippingDimensions.Equals(newDimensions))
                return;

            ShippingDimensions = newDimensions;
        }

        public void UpdateWarranty(int months)
        {
            if (months < 0)
                throw new DomainException("Срок гарантии указан неверно.");

            if (WarrantyMonths == months)
                return;

            WarrantyMonths = months;
        }

        public void ChangePrice(Money newPrice)
        {
            if (newPrice == null) throw new ArgumentNullException(
                nameof(newPrice));

            if (Price == newPrice) return;

            Money oldPrice = Price;
            Price = newPrice;

            AddDomainEvent(new ProductPriceChangedEvent(
                Id,
                oldPrice,
                newPrice));
        }

        public void DeductStock(int quantity)
        {
            if (quantity <= 0) throw new DomainException(
                "Количество должно быть больше нуля.");

            if (StockQuantity < quantity) throw new DomainException(
                "Недостаточно товара на складе.");

            StockQuantity -= quantity;
        }

        public void Restock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException(
                    "Количество должно быть больше нуля.");

            if (StockQuantity > int.MaxValue - quantity)
                throw new DomainException(
                    "Превышено максимальное количество товара.");

            StockQuantity += quantity;
        }

        public static Product Create(
            string name,
            string sku,
            string? gtin,
            string modelNumber,
            Money price,
            Guid categoryId,
            Guid brandId,
            string description,
            int warrantyMonths,
            Dimensions shippingDimensions,
            int initialStock = 0)

        {
            Product product = new Product(
                name,
                sku,
                gtin,
                modelNumber,
                price,
                categoryId,
                brandId,
                description,
                warrantyMonths,
                shippingDimensions,
                initialStock);

            // Добавляем событие в коллекцию агрегата
            product.AddDomainEvent(
                new ProductCreatedEvent(
                    product.Id,
                    product.Sku,
                    product.Price));

            return product;
        }

        public void Delete()
        {
            if (IsDeleted) return;

            IsDeleted = true;
            IsActive = false;
            DeletedAtUtc = DateTime.UtcNow;

            AddDomainEvent(new ProductDeletedEvent(Id));
        }

        // Для AddImage, RemoveImage
        private void SortImages()
        {
            _images.Sort(
                (x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder));
        }

        private static void ValidateBasicAttributes(
            string name,
            string sku,
            string modelNumber,
            Guid categoryId,
            Guid brandId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException(
                "Название товара обязательно.");
            if (string.IsNullOrWhiteSpace(sku)) throw new DomainException(
                "SKU товара обязателен.");
            if (string.IsNullOrWhiteSpace(modelNumber)) throw new DomainException(
                "Партномер (Model Number) обязателен для электроники.");
            if (categoryId == Guid.Empty) throw new DomainException(
                "Категория обязательна.");
            if (brandId == Guid.Empty) throw new DomainException(
                "Бренд обязателен.");
        }
    }
}