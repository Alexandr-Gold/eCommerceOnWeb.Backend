using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
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
        public Dimensions ShippingDimensions { get; private set; } = Dimensions.Zero(); // Габариты для СДЭК/Почты
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; } = true;

        // --- Связи по ID ---
        public Guid CategoryId { get; private set; }
        public Guid BrandId { get; private set; }

        // --- Инкапсулированные коллекции (DDD-стиль) ---
        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images.OrderBy(i => i.DisplayOrder).ToList().AsReadOnly();

        public ProductAttributes Attributes { get; private set; } = ProductAttributes.Empty();

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
            int warrantyMonths,
            Dimensions shippingDimensions,
            int initialStock = 0)
        {
            ValidateBasicAttributes(name, sku, modelNumber, categoryId, brandId);
            if (warrantyMonths < 0) throw new DomainException("Гарантия не может быть отрицательной.");

            Name = name;
            Sku = sku.ToUpper();
            Gtin = gtin;
            ModelNumber = modelNumber;
            Price = price ?? throw new ArgumentNullException(nameof(price));
            CategoryId = categoryId;
            BrandId = brandId;
            WarrantyMonths = warrantyMonths;
            ShippingDimensions = shippingDimensions ?? throw new ArgumentNullException(nameof(shippingDimensions));
            StockQuantity = initialStock;

            AddDomainEvent(new ProductCreatedEvent(Id, Sku, Price));
        }

        // --- МЕТОДЫ УПРАВЛЕНИЯ ИЗОБРАЖЕНИЯМИ (Инварианты галереи) ---

        public void AddImage(string url, string altText, int displayOrder, bool setAsMain = false)
        {
            if (_images.Any(i => i.Url.Equals(url, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("Такое изображение уже добавлено к товару.");

            ProductImage image = new ProductImage(url, altText, displayOrder);
            _images.Add(image);

            // Если это первое фото или передан флаг setAsMain — делаем его главным [1, 2]
            if (setAsMain || _images.Count == 1)
            {
                SetMainImage(image.Id);
            }
        }

        public void SetMainImage(Guid imageId)
        {
            ProductImage? targetImage = _images.FirstOrDefault(i => i.Id == imageId);
            if (targetImage == null) throw new DomainException("Изображение не найдено в галерее товара.");

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

            // Если удалили главное фото, автоматически назначаем главным первое оставшееся [1, 2]
            if (image.IsMain && _images.Any())
            {
                _images.First().IsMain = true;
            }
        }

        // --- ДРУГИЕ БИЗНЕС-МЕТОДЫ ---

        // Установить или обновить атрибут
        public void SetAttribute(string key, object value)
        {
            object? oldValue = GetAttribute<object>(key);
            Attributes = Attributes.SetValue(key, value);
            AddDomainEvent(new ProductAttributeChangedEvent(Id, key, oldValue, value));
        }

        // Удалить атрибут
        public void RemoveAttribute(string key)
        {
            Attributes = Attributes.RemoveValue(key);
            // Опционально: событие
        }

        // Получить значение атрибута (для внутренних нужд)
        public T? GetAttribute<T>(string key) => Attributes.GetValue<T>(key);

        public void UpdateShippingDimensions(Dimensions newDimensions)
        {
            ShippingDimensions = newDimensions ?? throw new ArgumentNullException(nameof(newDimensions));
        }

        private Dictionary<string, object> _specifications = new();
        public IReadOnlyDictionary<string, object> Specifications => _specifications.AsReadOnly();

        public void UpdateWarranty(int months)
        {
            if (months < 0) throw new DomainException("Срок гарантии указан неверно.");
            WarrantyMonths = months;
        }

        public void ChangePrice(Money newPrice)
        {
            if (newPrice == null) throw new ArgumentNullException(nameof(newPrice));
            if (Price.Equals(newPrice)) return;

            Money oldPrice = Price;
            Price = newPrice;

            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
        }

        public void DeductStock(int quantity)
        {
            if (quantity <= 0) throw new DomainException("Количество должно быть больше нуля.");
            if (StockQuantity < quantity) throw new DomainException("Недостаточно товара на складе.");
            StockQuantity -= quantity;
        }

        public void Restock(int quantity)
        {
            if (quantity <= 0) throw new DomainException("Количество должно быть больше нуля.");
            StockQuantity += quantity;
        }

        public void Delete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            IsActive = false;
            DeletedAtUtc = DateTime.UtcNow;
            AddDomainEvent(new ProductDeletedEvent(Id));
        }

        private static void ValidateBasicAttributes(string name, string sku, string modelNumber, Guid categoryId, Guid brandId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Имя товара обязательно.");
            if (string.IsNullOrWhiteSpace(sku)) throw new DomainException("SKU товара обязателен.");
            if (string.IsNullOrWhiteSpace(modelNumber)) throw new DomainException("Партномер (Model Number) обязателен для электроники.");
            if (categoryId == Guid.Empty) throw new DomainException("Категория обязательна.");
            if (brandId == Guid.Empty) throw new DomainException("Бренд обязателен.");
        }
    }
}