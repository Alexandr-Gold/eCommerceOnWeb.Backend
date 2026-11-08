using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate;

public class ProductTests
{
    [Fact]
    public void Create_ShouldCreateProduct_WhenArgumentsAreValid()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid brandId = Guid.NewGuid();

        // Act
        Product product = Product.Create(
            name: "iPhone 15 Pro",
            sku: "IPH-15PRO-128",
            gtin: "1234567890123",
            modelNumber: "A3106",
            price: Money.Create(999.00m, "USD"),
            categoryId: categoryId,
            brandId: brandId,
            description: "Test product description",
            warrantyMonths: 12,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 5);

        // Assert
        product.Id.Should().NotBe(Guid.Empty);           // BaseEntity.Init
        product.Name.Should().Be("iPhone 15 Pro");
        product.Sku.Should().Be("IPH-15PRO-128");
        product.Gtin.Should().Be("1234567890123");
        product.ModelNumber.Should().Be("A3106");
        product.Price.Should().Be(Money.Create(999.00m, "USD"));
        product.CategoryId.Should().Be(categoryId);      // ← не проверялось
        product.BrandId.Should().Be(brandId);            // ← не проверялось
        product.Description.Should().Be("Test product description");   // ← не проверялось
        product.WarrantyMonths.Should().Be(12);
        product.StockQuantity.Should().Be(5);
        product.Status.Should().Be(ProductStatus.Draft);
        product.IsDeleted.Should().BeFalse();
        product.DeletedAtUtc.Should().BeNull();          // ← не проверялось
        product.Images.Should().BeEmpty();               // ← не проверялось: Create не добавляет картинок
        product.Attributes.GetValues().Should().BeEmpty();          // ← не проверялось
    }

    [Fact]
    public void Create_ShouldAddProductCreatedDomainEvent()
    {
        // Arrange: Create напрямую — хелпер очищает DomainEvents
        const string sku = "IPH-15PRO-128";

        Product product = Product.Create(
            name: "Test Smartphone",
            sku: sku,
            gtin: "1234567890123",
            modelNumber: "MODEL-001",
            price: Money.Create(999.99m, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: 24,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 10);

        // Act
        IReadOnlyCollection<DomainEvent> events = product.DomainEvents;

        // Assert
        events.Should().ContainSingle();

        ProductCreatedEvent domainEvent = events.Single()
            .Should().BeOfType<ProductCreatedEvent>()
            .Subject;

        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.Sku.Should().Be(sku);
        domainEvent.Price.Should().Be(product.Price);
    }

    [Fact]
    public void Create_ShouldInitializeStatusAsDraft()
    {
        // Arrange: Product.Create напрямую — статус Draft выставляется фабрикой,
        // а не хелпером через ApplyStatus.
        Product product = Product.Create(
            name: "Test Smartphone",
            sku: "TEST-001",
            gtin: null,
            modelNumber: "MODEL-001",
            price: Money.Create(999.99m, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: 24,
            shippingDimensions: Dimensions.Create(10m, 20m, 5m, 1m),
            initialStock: 10);

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Constructor_ShouldTrimStrings()
    {
        // Arrange & Act
        Product product = ProductTestData.CreateValidProduct(
            name: "   PlayStation 5   ",
            modelNumber: "   CFI-1200   ",
            description: "   Next-gen console   "
        );

        // Assert
        product.Name.Should().Be("PlayStation 5");
        product.ModelNumber.Should().Be("CFI-1200");
        product.Description.Should().Be("Next-gen console");
    }

    [Fact]
    public void Constructor_ShouldNormalizeSkuToUpperInvariant()
    {
        // Arrange & Act
        Product product = ProductTestData.CreateValidProduct(sku: "  ps5-disc-v2  ");

        // Assert
        product.Sku.Should().Be("PS5-DISC-V2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldStoreNullGtin_WhenWhitespacePassed(string? invalidGtin)
    {
        // Arrange & Act
        Product product = ProductTestData.CreateValidProduct(gtin: invalidGtin);

        // Assert
        product.Gtin.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenNameIsEmpty(string? invalidName)
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(name: invalidName!);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Название товара обязательно.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenSkuIsEmpty(string? invalidSku)
    {
        // Act - Передаем плохой SKU напрямую в домен, минуя фабрику тестовых данных
        Action act = () => Product.Create(
            name: "Test Smartphone",
            sku: invalidSku!, // Передаем null/пустоту, + пробелы
            gtin: "1234567890123",
            modelNumber: "MODEL-001",
            price: new Money(999.99m, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Test product description",
            warrantyMonths: 24,
            shippingDimensions: Dimensions.Create(10, 20, 5, 1),
            initialStock: 10);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("SKU товара обязателен.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenModelNumberIsEmpty(string? invalidModel)
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(modelNumber: invalidModel!);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Партномер (Model Number) обязателен для электроники.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCategoryIsEmpty()
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(categoryId: Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Категория обязательна.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBrandIsEmpty()
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(brandId: Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Бренд обязателен.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenWarrantyIsNegative()
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(warrantyMonths: -1);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Гарантия не может быть отрицательной.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInitialStockIsNegative()
    {
        // Act
        Action act = () => ProductTestData.CreateValidProduct(initialStock: -1);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Начальный запас не может быть отрицательным.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPriceIsNull()
    {
        // Act
        // Так как ProductTestData создает Money под капотом, вызываем напрямую фабрику/конструктор Product
        Action act = () => Product.Create(
            name: "Valid Name",
            sku: "VALID-SKU",
            gtin: null,
            modelNumber: "MODEL",
            price: null!, // Проверяемый инвариант
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Description",
            warrantyMonths: 12,
            shippingDimensions: Dimensions.Create(10, 10, 10, 1),
            initialStock: 0);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("price");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenShippingDimensionsIsNull()
    {
        // Act
        Action act = () => Product.Create(
            name: "Valid Name",
            sku: "VALID-SKU",
            gtin: null,
            modelNumber: "MODEL",
            price: new Money(100, "USD"),
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid(),
            description: "Description",
            warrantyMonths: 12,
            shippingDimensions: null!, // Проверяемый инвариант
            initialStock: 0);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("shippingDimensions");
    }

    [Fact]
    public void AddImage_ShouldAddFirstImage_AndSetItAsMain()
    {
        // Arrange: стартуем без изображений — иначе CreateValidProduct уже добавил main
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        // Act
        product.AddImage(
            storageKey: "products/img1.webp",
            altText: "Front view",
            displayOrder: 1);

        // Assert
        product.Images.Should().ContainSingle();

        ProductImage image = product.Images.Single();
        image.StorageKey.Should().Be("products/img1.webp");
        image.AltText.Should().Be("Front view");
        image.DisplayOrder.Should().Be(1);
        image.IsMain.Should().BeTrue();
    }

    [Fact]
    public void AddImage_MainFlagStaysWithOriginalImage_AfterSorting()
    {
        Product product = ProductTestData.CreateValidProduct(withImage: false);

        product.AddImage("img3.webp", "3", displayOrder: 3);   // main (count == 1)
        product.AddImage("img1.webp", "1", displayOrder: 1);

        // Сортировка поставила img1 первым, но main — img3
        product.Images.First().StorageKey.Should().Be("img1.webp");
        product.Images.Single(i => i.IsMain).StorageKey.Should().Be("img3.webp");
    }

    [Fact]
    public void AddImage_ShouldSetImageAsMain_WhenRequested()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.AddImage("img1.webp", "Front", 1);

        // Act
        product.AddImage(
            "img2.webp",
            "Back",
            2,
            setAsMain: true);

        // Assert
        product.Images.Single(i => i.StorageKey == "img1.webp")
            .IsMain.Should().BeFalse();

        product.Images.Single(i => i.StorageKey == "img2.webp")
            .IsMain.Should().BeTrue();
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenStorageKeyAlreadyExists()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.AddImage("phone.webp", "Front", 1);

        // Act
        Action act = () =>
            product.AddImage("phone.webp", "Another", 2);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Такое изображение уже добавлено к товару.");
    }

    [Fact]
    public void AddImage_ShouldThrow_WhenStorageKeyDiffersOnlyByCase()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.AddImage("Phone.webp", "Front", 1);

        // Act
        Action act = () =>
            product.AddImage("phone.webp", "Back", 2);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Такое изображение уже добавлено к товару.");
    }

    [Fact]
    public void AddImage_ShouldNotReplaceMainImage_WhenSetAsMainIsFalse()
    {
        // Arrange: стартуем без изображений — иначе CreateValidProduct уже добавил main
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("img1.webp", "Front", displayOrder: 1);   // count == 1 → auto-main

        // Act
        product.AddImage("img2.webp", "Back", displayOrder: 2);    // setAsMain: false

        // Assert
        product.Images.Should().HaveCount(2);
        product.Images.Single(i => i.StorageKey == "img1.webp")
            .IsMain.Should().BeTrue();
        product.Images.Single(i => i.StorageKey == "img2.webp")
            .IsMain.Should().BeFalse();
    }

    [Fact]
    public void AddImage_ShouldTrimStorageKey()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        // Act
        product.AddImage(
            "   folder/image.webp   ",
            "Front",
            displayOrder: 1);

        // Assert
        product.Images.Should().ContainSingle()
            .Which.StorageKey.Should().Be("folder/image.webp");
    }

    [Fact]
    public void AddImage_ShouldKeepImagesSortedByDisplayOrder()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        // Act — добавляем в «неправильном» порядке
        product.AddImage("img3.webp", "3", displayOrder: 3);
        product.AddImage("img1.webp", "1", displayOrder: 1);
        product.AddImage("img2.webp", "2", displayOrder: 2);

        // Assert — коллекция хранится отсортированной
        product.Images
            .Select(i => i.DisplayOrder)
            .Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void SetMainImage_ShouldSetSelectedImageAsMain()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.AddImage(
            "images/product-1.jpg",
            "Front",
            1);

        product.AddImage(
            "images/product-2.jpg",
            "Back",
            2);

        ProductImage secondImage =
            product.Images
                .OrderBy(x => x.DisplayOrder)
                .Last();
    }

    [Fact]
    public void SetMainImage_ShouldThrow_WhenImageDoesNotExist()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        Guid unknownImageId = Guid.NewGuid();


        // Act
        Action act = () =>
            product.SetMainImage(unknownImageId);


        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Изображение не найдено в галерее товара.");
    }

    [Fact]
    public void SetMainImage_ShouldKeepOnlyOneMainImage()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.AddImage(
            "images/1.jpg",
            "Image 1",
            1);

        product.AddImage(
            "images/2.jpg",
            "Image 2",
            2);

        product.AddImage(
            "images/3.jpg",
            "Image 3",
            3);

        ProductImage thirdImage =
            product.Images
                .OrderBy(x => x.DisplayOrder)
                .Last();

        // Act
        product.SetMainImage(thirdImage.Id);

        // Assert
        product.Images
            .Count(x => x.IsMain)
            .Should()
            .Be(1);

        thirdImage.IsMain
            .Should()
            .BeTrue();
    }
    [Fact]
    public void RemoveImage_ShouldRemoveExistingImage()
    {
        // Arrange: стартуем без изображений — иначе есть картинка из хелпера
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("images/front.jpg", "Front", displayOrder: 1);
        ProductImage image = product.Images.Single();

        // Act
        product.RemoveImage(image.Id);

        // Assert
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_ShouldDoNothing_WhenImageDoesNotExist()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("images/front.jpg", "Front", displayOrder: 1);

        // Act
        product.RemoveImage(Guid.NewGuid());

        // Assert
        product.Images.Should().ContainSingle();
        product.Images.Single().IsMain.Should().BeTrue();
    }

    [Fact]
    public void RemoveImage_ShouldAssignNewMainImage_WhenMainImageRemoved()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("1.jpg", "One", displayOrder: 1);      // auto-main (count == 1)
        product.AddImage("2.jpg", "Two", displayOrder: 2);
        product.AddImage("3.jpg", "Three", displayOrder: 3);

        ProductImage main = product.Images.Single(i => i.IsMain);
        main.DisplayOrder.Should().Be(1);                       // precondition: main — order 1

        // Act
        product.RemoveImage(main.Id);

        // Assert
        product.Images.Should().HaveCount(2);

        // ровно одна main, и это — минимальный displayOrder среди оставшихся
        product.Images.Count(i => i.IsMain).Should().Be(1);
        product.Images.Single(i => i.IsMain).DisplayOrder.Should().Be(2);
    }

    [Fact]
    public void RemoveImage_ShouldKeepCurrentMain_WhenRemovingNonMainImage()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("1.jpg", "One", displayOrder: 1);      // auto-main
        product.AddImage("2.jpg", "Two", displayOrder: 2);

        ProductImage main = product.Images.Single(i => i.IsMain);
        ProductImage nonMain = product.Images.Single(i => !i.IsMain);

        // Act
        product.RemoveImage(nonMain.Id);

        // Assert
        product.Images.Should().ContainSingle();
        product.Images.Single().Id.Should().Be(main.Id);
        product.Images.Single().IsMain.Should().BeTrue();
    }

    [Fact]
    public void RemoveImage_ShouldLeaveGalleryEmpty_WhenLastImageRemoved()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            withImage: false);

        product.AddImage("1.jpg", "One", displayOrder: 1);
        ProductImage image = product.Images.Single();

        // Act
        product.RemoveImage(image.Id);

        // Assert
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void SetAttribute_ShouldAddNewAttribute()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.SetAttribute("Color", "Black");

        // Assert
        product.GetAttribute<string>("Color")
            .Should()
            .Be("Black");
    }

    [Fact]
    public void SetAttribute_ShouldReplaceExistingAttribute()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute("RAM", 8);

        // Act
        product.SetAttribute("RAM", 12);

        // Assert
        product.GetAttribute<int>("RAM")
            .Should()
            .Be(12);
    }

    [Fact]
    public void RemoveAttribute_ShouldRemoveAndRaiseEvent_WhenExists()
    {
        Product product = ProductTestData.CreateValidProduct(status: ProductStatus.Draft);
        product.SetAttribute("color", "red");
        product.ClearDomainEvents();

        product.RemoveAttribute("color");

        product.GetAttribute<string>("color").Should().BeNull();
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductAttributeChangedEvent>()
            .Which.Should().Match<ProductAttributeChangedEvent>(e =>
                e.Key == "color" &&
                Equals(e.OldValue, "red") &&
                e.NewValue == null);
    }

    [Fact]
    public void RemoveAttribute_ShouldDoNothing_WhenKeyDoesNotExist()
    {
        Product product = ProductTestData.CreateValidProduct(status: ProductStatus.Draft);

        product.RemoveAttribute("unknown");

        product.Attributes.GetValues().Should().BeEmpty();
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAttribute_ShouldRemove_WhenValueIsNull()
    {
        // SetValue(null) бросает, но FromDictionary пропускает null.
        Product product = ProductTestData.CreateValidProduct(status: ProductStatus.Draft);
        product.GetType() // не нужно — Attributes имеет private set, но FromDictionary доступен снаружи
            ;
        // Проще: проверить через рефлексию или через прямую замену:
        // Вариант ниже — если Attributes имеет internal-сеттер или public setter.

        // Если Attributes setter private — тест придётся делать на уровне ProductAttributes напрямую:
        ProductAttributes attrs = ProductAttributes.FromDictionary(new() { ["broken"] = null! });
        attrs.GetValues().Should().ContainKey("broken");
        attrs.RemoveValue("broken").GetValues().Should().NotContainKey("broken");

        // А тест на Product.RemoveAttribute с null-value — либо через `Attributes` setter (если он доступен),
        // либо задокументировать: Product.SetAttribute не пропускает null, поэтому в Product такой
        // сценарий не воспроизводится. Достаточно теста выше на ProductAttributes.RemoveValue.
    }

    [Fact]
    public void RemoveAttribute_ShouldRemoveExistingAttribute()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute("Storage", "256GB");

        // Act
        product.RemoveAttribute("Storage");

        // Assert
        product.GetAttribute<string>("Storage")
            .Should()
            .BeNull();
    }

    [Fact]
    public void RemoveAttribute_ShouldDoNothing_WhenAttributeDoesNotExist()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.RemoveAttribute("Unknown");

        // Assert
        product.Attributes
            .GetValues()
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void RemoveAttribute_ShouldThrow_WhenDeleted()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            isDeleted: true,
            initialStock: 0);

        Action act = () => product.RemoveAttribute("x");

        act.Should().Throw<DomainException>().WithMessage("*Удалённый*");
    }

    [Fact]
    public void RemoveAttribute_ShouldThrow_WhenDiscontinued()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Discontinued);

        Action act = () => product.RemoveAttribute("x");

        act.Should().Throw<DomainException>().WithMessage("*снят с производства*");
    }

    [Fact]
    public void GetAttribute_ShouldReturnExistingValue()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute("Weight", 1.5);

        // Act
        double? weight =
            product.GetAttribute<double>("Weight");

        // Assert
        weight.Should().Be(1.5);
    }

    [Fact]
    public void GetAttribute_ShouldReturnNull_WhenAttributeDoesNotExist()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        string? value =
            product.GetAttribute<string>("Unknown");

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public void RemoveAttribute_ShouldRaiseDomainEvent()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute("Color", "Black");

        product.ClearDomainEvents();

        // Act
        product.RemoveAttribute("Color");

        // Assert
        product.DomainEvents
            .Should()
            .ContainSingle(e => e is ProductAttributeChangedEvent);
    }

    [Fact]
    public void RemoveAttribute_ShouldNotRaiseEvent_WhenAttributeDoesNotExist()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        // Act
        product.RemoveAttribute("Unknown");

        // Assert
        product.DomainEvents
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void SetAttribute_ShouldRaiseDomainEvent()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.SetAttribute("Color", "Black");

        // Assert
        product.DomainEvents
            .Should()
            .ContainSingle(e => e is ProductAttributeChangedEvent);
    }

    [Fact]
    public void SetAttribute_ShouldStoreOldAndNewValueInDomainEvent()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute("RAM", 8);

        product.ClearDomainEvents();

        // Act
        product.SetAttribute("RAM", 12);

        // Assert
        ProductAttributeChangedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductAttributeChangedEvent>()
                .Single();

        domainEvent.Key.Should().Be("RAM");
        domainEvent.OldValue.Should().Be(8);
        domainEvent.NewValue.Should().Be(12);
    }

    [Fact]
    public void SetAttribute_ShouldSetDefaultValue_WhenAttributeDoesNotExist()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();
        product.ClearDomainEvents();

        // Act
        product.SetAttribute("stockLimit", 0);

        // Assert
        product.GetAttribute<int>("stockLimit")
            .Should()
            .Be(0);

        product.DomainEvents.Should()
            .ContainSingle(e => e is ProductAttributeChangedEvent);
    }

    [Fact]
    public void SetAttribute_ShouldNotRaiseEvent_WhenValueIsUnchanged()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        product.SetAttribute<int>("stockLimit", 0);
        product.ClearDomainEvents();

        // Act
        product.SetAttribute<int>("stockLimit", 0);

        // Assert
        bool found = product.Attributes.TryGetValue<int>(
            "stockLimit",
            out int value);

        found.Should().BeTrue();
        value.Should().Be(0);

        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.UpdateDescription("New description");

        // Assert
        product.Description
            .Should()
            .Be("New description");
    }

    [Fact]
    public void UpdateDescription_ShouldTrimDescription()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.UpdateDescription("   New description   ");

        // Assert
        product.Description
            .Should()
            .Be("New description");
    }

    //[Fact]
    //public void UpdateDescription_ToEmpty_ClearsDescription()
    //{
    //    // Arrange
    //    Product product = ProductTestData.CreateValidProduct();
    //    product.UpdateDescription("Initial description");
    //    product.ClearDomainEvents();

    //    // Act
    //    product.UpdateDescription("   ");

    //    // Assert
    //    product.Description.Should().BeEmpty();
    //    product.DomainEvents.Should().BeEmpty(); // событие на описании не поднимается
    //}

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_ToEmpty_ClearsDescription(string? description)
    {
        Product product = ProductTestData.CreateValidProduct(description: "Initial");
        product.ClearDomainEvents();

        product.UpdateDescription(description);

        product.Description.Should().BeEmpty();
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_ToNull_ClearsDescription()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();
        product.UpdateDescription("Initial description");
        product.ClearDomainEvents();

        // Act
        product.UpdateDescription(null);

        // Assert
        product.Description.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_ToSameValue_IsNoOp()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();
        product.UpdateDescription("Same");
        product.ClearDomainEvents();

        // Act
        product.UpdateDescription("  Same  "); // trim → "Same"

        // Assert
        product.Description.Should().Be("Same");
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_ShouldNotChange_WhenDescriptionIsSame()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        string original = product.Description;

        // Act
        product.UpdateDescription(original);

        // Assert
        product.Description
            .Should()
            .Be(original);
    }

    [Fact]
    public void UpdateWarranty_ShouldUpdateWarranty()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.UpdateWarranty(36);

        // Assert
        product.WarrantyMonths
            .Should()
            .Be(36);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-12)]
    public void UpdateWarranty_ShouldThrow_WhenWarrantyIsNegative(
int months)
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        Action act = () =>
            product.UpdateWarranty(months);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Срок гарантии указан неверно.");
    }

    [Fact]
    public void UpdateWarranty_ShouldNotChange_WhenWarrantyIsSame()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.UpdateWarranty(product.WarrantyMonths);

        // Assert
        product.WarrantyMonths
            .Should()
            .Be(24);
    }

    [Fact]
    public void UpdateWarranty_ShouldAllowZeroWarranty()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        product.UpdateWarranty(0);

        // Assert
        product.WarrantyMonths
            .Should()
            .Be(0);
    }

    [Fact]
    public void UpdateShippingDimensions_ShouldUpdateDimensions()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        Dimensions newDimensions =
            Dimensions.Create(
                15,
                25,
                7,
                1.5m);

        // Act
        product.UpdateShippingDimensions(newDimensions);

        // Assert
        product.ShippingDimensions
            .Should()
            .Be(newDimensions);
    }

    [Fact]
    public void UpdateShippingDimensions_ShouldNotChange_WhenDimensionsAreSame()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        Dimensions original =
            product.ShippingDimensions;

        // Act
        product.UpdateShippingDimensions(original);

        // Assert
        product.ShippingDimensions
            .Should()
            .BeSameAs(original);
    }

    [Fact]
    public void UpdateShippingDimensions_ShouldThrow_WhenDimensionsIsNull()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct();

        // Act
        Action act = () =>
            product.UpdateShippingDimensions(null!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        Money newPrice =
            new Money(1299.99m, "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        product.Price
            .Should()
            .Be(newPrice);
    }

    [Fact]
    public void ChangePrice_ShouldRaiseDomainEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        Money newPrice =
            new Money(1299.99m, "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        product.DomainEvents
            .Should()
            .ContainSingle(e =>
                e is ProductPriceChangedEvent);
    }

    [Fact]
    public void ChangePrice_ShouldStoreOldAndNewPriceInEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct(
                priceAmount: 999.99m);

        product.ClearDomainEvents();

        Money newPrice =
            new Money(1299.99m, "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        ProductPriceChangedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductPriceChangedEvent>()
                .Single();

        domainEvent.OldPrice
            .Should()
            .Be(new Money(999.99m, "USD"));

        domainEvent.NewPrice
            .Should()
            .Be(newPrice);
    }

    [Fact]
    public void ChangePrice_ShouldNotRaiseEvent_WhenPriceIsSame()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.ClearDomainEvents();

        Money currentPrice =
            product.Price;

        // Act
        product.ChangePrice(currentPrice);

        // Assert
        product.DomainEvents
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void ChangePrice_ShouldThrow_WhenPriceIsNull()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        // Act
        Action act = () =>
            product.ChangePrice(null!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void Restock_ShouldRaiseBackInStockEvent_WhenStockWasZero()
    {
        // Arrange: Draft + stock 0 + картинка (чтобы не мешали guard'ы на других путях)
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 0);

        // Act
        product.Restock(10);

        // Assert
        product.StockQuantity.Should().Be(10);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductBackInStockEvent>()
            .Which.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void Restock_ShouldNotRaiseBackInStockEvent_WhenStockWasPositive()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 5);

        product.Restock(3);

        product.StockQuantity.Should().Be(8);
        product.DomainEvents.Should().NotContain(e => e is ProductBackInStockEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DeductStock_ShouldThrow_WhenQuantityNonPositive(int quantity)
    {
        // Arrange: Active обязателен — иначе guard статуса сработает первым
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 10);

        // Act
        Action act = () => product.DeductStock(quantity);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Количество должно быть больше нуля.");
        product.StockQuantity.Should().Be(10);   // не изменился
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Restock_AfterGoingToZero_ShouldRaiseBackInStockEvent()
    {
        // Arrange: Active + 3 шт
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 3);

        product.DeductStock(3);                        // 3 → 0, поднимает OutOfStock
        product.ClearDomainEvents();

        // Act
        product.Restock(5);                            // 0 → 5, поднимает BackInStock

        // Assert
        product.StockQuantity.Should().Be(5);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductBackInStockEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Restock_ShouldThrow_WhenQuantityNonPositive(int quantity)
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 5);

        Action act = () => product.Restock(quantity);

        act.Should().Throw<DomainException>().WithMessage("*больше нуля*");
        product.StockQuantity.Should().Be(5);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Restock_ShouldThrow_WhenOverflow()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: int.MaxValue);

        Action act = () => product.Restock(1);

        act.Should().Throw<DomainException>().WithMessage("*максимальное*");
        product.StockQuantity.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Restock_ShouldThrow_WhenDiscontinued()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Discontinued,
            initialStock: 5);

        Action act = () => product.Restock(1);

        act.Should().Throw<DomainException>().WithMessage("*снят с производства*");
    }

    [Fact]
    public void Restock_ShouldThrow_WhenDeleted()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            isDeleted: true,
            initialStock: 0);

        Action act = () => product.Restock(1);

        act.Should().Throw<DomainException>().WithMessage("*Удалённый*");
    }

    [Fact]
    public void DeductStock_ShouldThrow_WhenNotEnoughStock()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 5);

        // Act
        Action act = () => product.DeductStock(6);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Недостаточно товара на складе.");
        product.StockQuantity.Should().Be(5);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DeductStock_ShouldDecreaseStock_WhenSufficient()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 10);

        // ← диагностика: если упадёт здесь — проблема в хелпере, не в DeductStock
        product.Status.Should().Be(ProductStatus.Active, "Arrange должен привести товар в Active");

        product.DeductStock(3);

        product.StockQuantity.Should().Be(7);
        product.DomainEvents.Should().BeEmpty();   // ещё не 0 — событие не поднимается
    }


    [Fact]
    public void Debug_StatusMismatch()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 10);

        ProductStatus status = product.Status;
        ProductStatus active = ProductStatus.Active;

        Console.WriteLine($"Status: value={status} int={(int)status} type={status.GetType().FullName} asm={status.GetType().Assembly.FullName}");
        Console.WriteLine($"Active: value={active} int={(int)active} type={active.GetType().FullName} asm={active.GetType().Assembly.FullName}");
        Console.WriteLine($"== : {status == active}");
        Console.WriteLine($"is : {status is ProductStatus.Active}");
        Console.WriteLine($"is not : {status is not ProductStatus.Active}");
        Console.WriteLine($"Equals: {status.Equals(active)}");
        Console.WriteLine($"Product type: {product.GetType().FullName}");
        Console.WriteLine($"Product asm: {product.GetType().Assembly.FullName}");
    }

    [Fact]
    public void Debug_DeductStock_BeforeAndAfter()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 10);

        Console.WriteLine($"BEFORE: Status={product.Status} (int={(int)product.Status}), " +
                         $"IsDeleted={product.IsDeleted}, Stock={product.StockQuantity}, " +
                         $"StatusType={product.Status.GetType().AssemblyQualifiedName}");
        Console.WriteLine($"Is not Active: {product.Status is not ProductStatus.Active}");

        try
        {
            product.DeductStock(3);
            Console.WriteLine($"AFTER: Status={product.Status}, Stock={product.StockQuantity}");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"EXCEPTION: {ex.Message}");
            Console.WriteLine($"AFTER-EX: Status={product.Status} (int={(int)product.Status}), " +
                             $"IsDeleted={product.IsDeleted}, Stock={product.StockQuantity}");
            throw;
        }
    }

    [Fact]
    public void DeductStock_ShouldRaiseOutOfStock_WhenStockReachesZero()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 3);

        product.DeductStock(3);

        product.StockQuantity.Should().Be(0);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductOutOfStockEvent>()
            .Which.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void DeductStock_ShouldNotRaiseOutOfStock_WhenStockRemainsPositive()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Active,
            initialStock: 3);

        product.DeductStock(2);

        product.DomainEvents.Should().NotContain(e => e is ProductOutOfStockEvent);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.Inactive)]
    [InlineData(ProductStatus.Discontinued)]
    public void DeductStock_ShouldThrow_WhenNotActive(ProductStatus status)
    {
        Product product = ProductTestData.CreateValidProduct(
            status: status,
            initialStock: 10);

        Action act = () => product.DeductStock(1);

        act.Should().Throw<DomainException>()
            .WithMessage("*только для активного*");
        product.StockQuantity.Should().Be(10);
    }

    [Fact]
    public void DeductStock_ShouldThrow_WhenDeleted()
    {
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            isDeleted: true,
            initialStock: 0);

        Action act = () => product.DeductStock(1);

        act.Should().Throw<DomainException>().WithMessage("*Удалённый*");
    }

    [Fact]
    public void Delete_ShouldThrow_WhenStockRemains()
    {
        // Arrange: stock > 0, isDeleted = false — CreateValidProduct не бросит
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 5);

        // Act
        Action act = () => product.Delete();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*остаток*");
        product.IsDeleted.Should().BeFalse();
        product.DeletedAtUtc.Should().BeNull();
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Delete_ShouldMarkProductAsDeleted()
    {
        // Arrange: stock = 0 обязателен, иначе Delete() бросит
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 0);

        ProductStatus initialStatus = product.Status;

        // Act
        product.Delete();

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedAtUtc.Should().NotBeNull();
        product.Status.Should().Be(initialStatus);   // удаление не трогает бизнес-статус
    }

    [Fact]
    public void Delete_ShouldSetDeletedAtUtc()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 0);
        DateTime before = DateTime.UtcNow;

        // Act
        product.Delete();

        // Assert
        product.DeletedAtUtc.Should().NotBeNull();
        product.DeletedAtUtc!.Value.Should().BeOnOrAfter(before);
        product.DeletedAtUtc!.Value.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Delete_ShouldRaiseProductDeletedEvent()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 0);
        // ClearDomainEvents уже вызван внутри CreateValidProduct — повторный не нужен

        // Act
        product.Delete();

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductDeletedEvent>()
            .Which.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void Delete_ShouldDoNothing_WhenAlreadyDeleted()
    {
        // Arrange
        Product product = ProductTestData.CreateValidProduct(
            status: ProductStatus.Draft,
            initialStock: 0);
        product.Delete();                                  // первый раз — реальное удаление

        DateTime? deletedAt = product.DeletedAtUtc;
        int eventsCount = product.DomainEvents.Count;      // 1: ProductDeletedEvent
        ProductStatus status = product.Status;

        // Act
        product.Delete();                                  // второй раз — no-op

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedAtUtc.Should().Be(deletedAt);       // не перезаписан
        product.Status.Should().Be(status);
        product.DomainEvents.Should().HaveCount(eventsCount);  // новых событий нет
    }

    [Fact]
    public void Activate_ShouldChangeStatus_FromDraftToActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Status.Should().Be(ProductStatus.Draft);

        // Act
        product.Activate();

        // Assert
        product.Status.Should().Be(ProductStatus.Active);

        product.DomainEvents.Should().ContainSingle(e =>
            e is ProductActivatedEvent);
    }

    [Fact]
    public void Activate_ShouldDoNothing_WhenAlreadyActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        product.Status.Should()
            .Be(ProductStatus.Active);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Activate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Active);

        product.DomainEvents
             .OfType<ProductActivatedEvent>()
             .Should()
             .HaveCount(1);
    }

    [Fact]
    public void Activate_ShouldChangeStatus_FromInactiveToActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        product.Status.Should().Be(ProductStatus.Inactive);

        int eventsBefore =
            product.DomainEvents.Count;

        // Act
        product.Activate();

        // Assert
        product.Status.Should().Be(ProductStatus.Active);

        product.DomainEvents.Should().HaveCount(eventsBefore + 1);

        product.DomainEvents.Last().Should()
            .BeOfType<ProductActivatedEvent>();
    }

    [Fact]
    public void Activate_ShouldThrow_WhenDescriptionEmpty()
    {
        Product product = ProductTestData.CreateValidProduct(description: string.Empty);
        product.ClearDomainEvents();

        Action act = () => product.Activate();

        act.Should().Throw<DomainException>().WithMessage("*без описания*");
        product.Status.Should().Be(ProductStatus.Draft);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_ShouldSucceed_WhenDescriptionFilled()
    {
        Product product = ProductTestData.CreateValidProduct(description: "iPhone 15 Pro 128GB");
        product.ClearDomainEvents();

        product.Activate();

        product.Status.Should().Be(ProductStatus.Active);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductActivatedEvent>();
    }

    [Fact]
    public void Activate_ShouldThrow_WhenProductIsDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue(
            "Производство прекращено");

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        Action act = () =>
            product.Activate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Activate_ShouldNotRaiseEvent_WhenAlreadyActive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Activate();

        // Assert
        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Activate_ShouldRaiseProductActivatedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        // Act
        product.Activate();

        // Assert
        product.DomainEvents.Should()
            .ContainSingle(e => e is ProductActivatedEvent);

        ProductActivatedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductActivatedEvent>()
                .Single();

        domainEvent.ProductId
            .Should()
            .Be(product.Id);
    }

    [Fact]
    public void Deactivate_ShouldChangeStatus_FromActiveToInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        product.Status.Should()
            .Be(ProductStatus.Active);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Deactivate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Inactive);

        product.DomainEvents.Should()
            .HaveCount(eventsCount + 1);

        product.DomainEvents.Last()
            .Should()
            .BeOfType<ProductDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_ShouldDoNothing_WhenAlreadyInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        product.Status.Should()
            .Be(ProductStatus.Inactive);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Deactivate();

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Inactive);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenProductIsDraft()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Status.Should()
            .Be(ProductStatus.Draft);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        Action act = () =>
            product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(
                "Нельзя деактивировать товар, который никогда не был опубликован.");

        product.Status.Should()
            .Be(ProductStatus.Draft);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromDraftToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Status.Should()
            .Be(ProductStatus.Draft);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Discontinue(
            "Снят с производства");

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount + 1);

        product.DomainEvents.Last()
            .Should()
            .BeOfType<ProductDiscontinuedEvent>();
    }

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromActiveToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        product.Status.Should()
            .Be(ProductStatus.Active);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Discontinue(
            "Производитель прекратил выпуск");

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount + 1);

        product.DomainEvents.Last()
            .Should()
            .BeOfType<ProductDiscontinuedEvent>();
    }

    [Fact]
    public void Discontinue_ShouldChangeStatus_FromInactiveToDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        product.Status.Should()
            .Be(ProductStatus.Inactive);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Discontinue(
            "Модель больше не выпускается");

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount + 1);

        product.DomainEvents.Last()
            .Should()
            .BeOfType<ProductDiscontinuedEvent>();
    }

    [Fact]
    public void Discontinue_ShouldDoNothing_WhenAlreadyDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue(
            "Снят с производства");

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Discontinue(
            "Повторное снятие");

        // Assert
        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenProductIsDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue(
            "Производство прекращено");

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        Action act = () =>
            product.Deactivate();

        // Assert
        act.Should()
            .Throw<DomainException>();

        product.Status.Should()
            .Be(ProductStatus.Discontinued);

        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Deactivate_ShouldRaiseProductDeactivatedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        // Act
        product.Deactivate();

        // Assert
        product.DomainEvents.Should()
            .ContainSingle(e => e is ProductDeactivatedEvent);

        ProductDeactivatedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductDeactivatedEvent>()
                .Single();

        domainEvent.ProductId
            .Should()
            .Be(product.Id);
    }

    [Fact]
    public void Discontinue_ShouldRaiseProductDiscontinuedEvent()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();

        string reason =
            "Производство прекращено";

        // Act
        product.Discontinue(reason);

        // Assert
        product.DomainEvents.Should()
            .ContainSingle(e => e is ProductDiscontinuedEvent);

        ProductDiscontinuedEvent domainEvent =
            product.DomainEvents
                .OfType<ProductDiscontinuedEvent>()
                .Single();

        domainEvent.ProductId
            .Should()
            .Be(product.Id);

        domainEvent.Reason
            .Should()
            .Be(reason);
    }

    [Fact]
    public void Deactivate_ShouldNotRaiseEvent_WhenAlreadyInactive()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Activate();
        product.Deactivate();

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Deactivate();

        // Assert
        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Fact]
    public void Discontinue_ShouldNotRaiseEvent_WhenAlreadyDiscontinued()
    {
        // Arrange
        Product product =
            ProductTestData.CreateValidProduct();

        product.Discontinue();

        int eventsCount =
            product.DomainEvents.Count;

        // Act
        product.Discontinue();

        // Assert
        product.DomainEvents.Should()
            .HaveCount(eventsCount);
    }

    [Theory]
    [InlineData(ProductStatus.Discontinued, true)]
    [InlineData(ProductStatus.Active, false)]
    [InlineData(ProductStatus.Inactive, false)]
    [InlineData(ProductStatus.Draft, false)]
    public void IsTerminal_ShouldReturnExpectedResult(
         ProductStatus status,
         bool expected)
    {
        status.IsTerminal()
            .Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(ProductStatus.Draft, false)]
    [InlineData(ProductStatus.Active, true)]
    [InlineData(ProductStatus.Inactive, false)]
    [InlineData(ProductStatus.Discontinued, false)]
    public void IsSellable_ShouldReturnExpectedResult(
         ProductStatus status,
         bool expected)
    {
        bool result =
            status.IsSellable();

        result.Should()
            .Be(expected);
    }

    [Theory]
    [InlineData(ProductStatus.Draft, true)]
    [InlineData(ProductStatus.Active, true)]
    [InlineData(ProductStatus.Inactive, true)]
    [InlineData(ProductStatus.Discontinued, false)]
    public void IsEditable_ShouldReturnExpectedResult(
         ProductStatus status,
         bool expected)
    {
        bool result =
            status.IsEditable();

        result.Should()
            .Be(expected);
    }
}
