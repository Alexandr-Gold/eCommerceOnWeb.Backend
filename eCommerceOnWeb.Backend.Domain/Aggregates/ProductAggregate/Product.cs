using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;

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

    // Status описывает бизнес-состояние публикации товара enum.
    // При усложнении жизненного цикла возможен переход на Smart Enum.
    // Статуса Editing нет: редактирование выполняется
    // в текущем состоянии товара.
    // Draft, Active и Inactive могут изменяться согласно
    // правилам агрегата.
    // Discontinued и Deleted запрещают изменения.
    public ProductStatus Status { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid BrandId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images =>
        _images.AsReadOnly();

    public ProductAttributes Attributes { get; private set; }
        = ProductAttributes.Empty();

    // --- Софт-делит ---
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Product() { } // Для EF Core

    private Product(
        string name,
        string sku,
        string? gtin,
        string modelNumber,
        Money price,
        Guid categoryId,
        Guid brandId,
        string? description,
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
        Gtin = string.IsNullOrWhiteSpace(gtin) ? null : gtin.Trim();
        ModelNumber = modelNumber.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CategoryId = categoryId;
        BrandId = brandId;
        Description = description?.Trim() ?? string.Empty;
        WarrantyMonths = warrantyMonths;
        ShippingDimensions = shippingDimensions ?? throw new ArgumentNullException(nameof(shippingDimensions));
        StockQuantity = initialStock;
        Status = ProductStatus.Draft;     // Начальное состояние
    }

    // --- МЕТОДЫ УПРАВЛЕНИЯ ИЗОБРАЖЕНИЯМИ (Инварианты галереи) ---

    public void AddImage(
        string storageKey,
        string altText,
        int displayOrder,
        bool setAsMain = false)
    {
        ThrowIfNotEditable();

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException(
                "Storage Key изображения обязателен.");

        // Нормализуем ключ до любых проверок, чтобы отсечь скрытые дубликаты с пробелами
        string normalizedStorageKey = storageKey.Trim();

        if (_images.Any(i => i.StorageKey.Equals(normalizedStorageKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
                "Такое изображение уже добавлено к товару.");
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
        ThrowIfNotEditable();

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
        ThrowIfNotEditable();

        ProductImage? image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null) return;

        if (Status is ProductStatus.Active && _images.Count == 1)
            throw new DomainException(
                "Нельзя удалить единственное изображение активного товара.");

        _images.Remove(image);

        if (image.IsMain && _images.Count > 0)
            _images[0].IsMain = true;
    }

    // --- ДРУГИЕ БИЗНЕС-МЕТОДЫ ---

    /// <summary>
    /// Обновляет описание товара. Пустое описание (null / "" / whitespace)
    /// допустимо в Draft и очищает поле. Проверка «нельзя публиковать
    /// без описания» выполняется в <see cref="Activate"/>.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        ThrowIfNotEditable();

        string normalized = description?.Trim() ?? string.Empty;

        if (Status is ProductStatus.Active && normalized.Length == 0)
            throw new DomainException(
                "Нельзя очистить описание активного товара.");

        if (Description.Equals(normalized, StringComparison.Ordinal))
            return;

        Description = normalized;
    }

    // Установить или обновить атрибут
    public void SetAttribute<T>(string key, T value)
     where T : notnull
    {
        ThrowIfNotEditable();

        // Проверяем, существует ли атрибут
        // и можно ли получить его в нужном типе.
        bool hasTypedOldValue =
            Attributes.TryGetValue<T>(
                key,
                out T? oldValue);

        // Получаем исходное значение для Domain Event.
        bool keyExists =
            Attributes.TryGetRawValue(
                key,
                out object? rawOldValue);

        // Если значение фактически не изменилось,
        // ничего не делаем.
        if (hasTypedOldValue
            && EqualityComparer<T>.Default.Equals(
                oldValue!,
                value))
            return;


        // Если удалось получить типизированное значение,
        // используем его; иначе сохраняем исходное значение.
        object? eventOldValue = hasTypedOldValue
            ? oldValue
            : keyExists ? rawOldValue : null;

        Attributes = Attributes.SetValue(key, value);

        AddDomainEvent(
            new ProductAttributeChangedEvent(
                Id,
                key,
                eventOldValue,
                value));
    }

    // Удалить атрибут
    public void RemoveAttribute(string key)
    {
        ThrowIfNotEditable();

        //object? oldValue = GetAttribute<object>(key);

        //if (oldValue is null)
        //    return;

        if (!Attributes.TryGetRawValue(key, out object? oldValue))
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
        ThrowIfNotEditable();

        ArgumentNullException.ThrowIfNull(newDimensions);

        if (ShippingDimensions.Equals(newDimensions))
            return;

        ShippingDimensions = newDimensions;
    }

    public void UpdateWarranty(int months)
    {
        ThrowIfNotEditable();

        if (months < 0)
            throw new DomainException("Срок гарантии указан неверно.");

        if (WarrantyMonths == months)
            return;

        WarrantyMonths = months;
    }

    public void ChangePrice(Money newPrice)
    {
        ThrowIfNotEditable();

        ArgumentNullException.ThrowIfNull(newPrice);

        if (!newPrice.IsPositive)
            throw new DomainException("Цена должна быть больше нуля.");

        if (Price == newPrice)
            return;

        Money oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedEvent(
            Id,
            oldPrice,
            newPrice));
    }

    public void DeductStock(int quantity)
    {
        ThrowIfDeleted();

        if (Status is not ProductStatus.Active)
            throw new DomainException(
                "Списание возможно только для активного товара.");

        if (quantity <= 0) throw new DomainException(
            "Количество должно быть больше нуля.");

        if (StockQuantity < quantity) throw new DomainException(
            "Недостаточно товара на складе.");

        StockQuantity -= quantity;

        if (StockQuantity == 0)
        {
            AddDomainEvent(
                new ProductOutOfStockEvent(Id));
        }
    }

    public void Restock(int quantity)
    {
        ThrowIfNotEditable();

        if (quantity <= 0)
            throw new DomainException(
                "Количество должно быть больше нуля.");

        if (StockQuantity > int.MaxValue - quantity)
            throw new DomainException(
                "Превышено максимальное количество товара.");

        bool wasOutOfStock = StockQuantity == 0;
        StockQuantity += quantity;

        // Если товар реально лежал на нуле, а теперь появился — генерируем событие
        if (wasOutOfStock)
            AddDomainEvent(new ProductBackInStockEvent(Id));
    }

    public static Product Create(
        string name,
        string sku,
        string? gtin,
        string modelNumber,
        Money price,
        Guid categoryId,
        Guid brandId,
        string? description,
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
        if (IsDeleted)
            return;

        if (!Status.IsTerminal() && StockQuantity > 0)     // ← только НЕтерминальный с остатком
            throw new DomainException(
                "Нельзя удалить товар, пока на складе есть остаток. " +
                "Сначала спишите остаток или снимите товар с производства.");

        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new ProductDeletedEvent(Id));
    }

    /// <summary>
    /// Публикует товар. Разрешено из Draft и Inactive.
    /// Требования к публикации: непустое описание, цена &gt; 0,
    /// хотя бы одно изображение.
    /// Идемпотентно: повторный вызов для Active — no-op.
    /// </summary>
    public void Activate()
    {
        ThrowIfDeleted();

        if (Status is ProductStatus.Active)
            return;

        if (Status.IsTerminal())
            throw new DomainException(
                "Снятый с производства товар нельзя снова активировать.");

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException(
                "Нельзя опубликовать товар без описания.");

        if (!Price.IsPositive)
            throw new DomainException(
                "Нельзя опубликовать товар без цены.");

        if (_images.Count == 0)
            throw new DomainException(
                "Нельзя опубликовать товар без изображения.");

        Status = ProductStatus.Active;

        AddDomainEvent(
            new ProductActivatedEvent(Id));
    }

    public void Deactivate()
    {
        ThrowIfDeleted();

        if (Status is ProductStatus.Inactive)
            return;

        if (Status is ProductStatus.Draft)
        {
            throw new DomainException(
                "Нельзя деактивировать товар, который никогда не был опубликован.");
        }

        if (Status.IsTerminal())
        {
            throw new DomainException(
                "Нельзя деактивировать снятый с производства товар.");
        }

        Status = ProductStatus.Inactive;

        AddDomainEvent(
            new ProductDeactivatedEvent(Id));
    }

    /// <summary>
    /// Окончательно снимает товар с продажи.
    /// Разрешено из любого нетерминального статуса
    /// (Draft, Active, Inactive).
    ///
    /// Повторный вызов является no-op.
    ///
    /// StockQuantity не изменяется.
    /// Списание или иное управление остатком выполняется
    /// отдельным процессом.
    /// </summary>
    public void Discontinue(string? reason = null)
    {
        ThrowIfDeleted();

        if (Status.IsTerminal())
            return;

        Status = ProductStatus.Discontinued;

        AddDomainEvent(
            new ProductDiscontinuedEvent(
                Id,
                reason));
    }

    // Для AddImage, RemoveImage
    private void SortImages()
    {
        _images.Sort(
            (x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder));
    }

    /// <summary>
    /// Проверяет операции, которые разрешены только
    /// для неопубликованных товаров.
    /// Active-товары уже опубликованы и не должны
    /// менять критические данные карточки.
    /// </summary>
    private void ThrowIfPublished()
    {
        ThrowIfNotEditable();

        if (Status == ProductStatus.Active)
            throw new DomainException(
                "Опубликованный товар нельзя изменять в этой операции.");
    }

    /// <summary>
    /// Проверяет, что товар допускает изменения.
    /// Удалённые и терминальные товары не могут быть изменены.
    /// Draft, Active и Inactive проходят эту проверку.
    /// </summary>
    private void ThrowIfNotEditable()
    {
        ThrowIfDeleted();
        ThrowIfDiscontinued();
    }

    private void ThrowIfDeleted()
    {
        if (IsDeleted)
            throw new DomainException(
                "Удалённый товар нельзя изменить.");
    }

    /// <summary>
    /// Бросает, если товар снят с производства.
    /// Сейчас Discontinued — единственный терминальный статус,
    /// поэтому проверка через IsTerminal() эквивалентна проверке на Discontinued.
    /// При добавлении второго терминального статуса — переименовать в ThrowIfTerminal
    /// и решить, должно ли новое состояние блокировать изменения.
    /// </summary>
    private void ThrowIfDiscontinued()
    {
        if (Status.IsTerminal())
            throw new DomainException(
                "Товар снят с производства и не может быть изменён.");
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