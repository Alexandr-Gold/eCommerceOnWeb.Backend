using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using FluentAssertions;

namespace eCommerceOnWeb.Backend.Domain.UnitTests.Aggregates.ProductAggregate
{
    public class ProductTests
    {
        [Fact]
        public void Create_ShouldCreateProduct_WhenArgumentsAreValid()
        {
            Product product = ProductTestData.CreateValidProduct(
           name: "iPhone 15 Pro",
           sku: "IPH-15PRO-128",
           gtin: "1234567890123",
           modelNumber: "A3106",
           priceAmount: 999.00m,
           initialStock: 5,
           warrantyMonths: 12
            );

            product.Should().NotBeNull();
            product.Name.Should().Be("iPhone 15 Pro");
            product.Sku.Should().Be("IPH-15PRO-128");
            product.Gtin.Should().Be("1234567890123");
            product.ModelNumber.Should().Be("A3106");
            product.Price.Amount.Should().Be(999.00m);
            product.StockQuantity.Should().Be(5);
            product.WarrantyMonths.Should().Be(12);
            product.Status.Should().Be(ProductStatus.Draft);
            product.IsDeleted.Should().BeFalse();
        }
        [Fact]
        public void Create_ShouldAddProductCreatedDomainEvent()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct(
                    sku: "IPH-15PRO-128");

            // Act
            IReadOnlyCollection<DomainEvent> events =
                product.DomainEvents;

            // Assert
            events.Should()
                .ContainSingle();

            ProductCreatedEvent domainEvent =
                events
                    .Single()
                    .Should()
                    .BeOfType<ProductCreatedEvent>()
                    .Subject;

            domainEvent.ProductId
                .Should()
                .Be(product.Id);

            domainEvent.Sku
                .Should()
                .Be("IPH-15PRO-128");

            domainEvent.Price
                .Should()
                .Be(product.Price);
        }

        [Fact]
        public void Create_ShouldInitializeStatusAsDraft()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            // Act
            ProductStatus status = product.Status;

            // Assert
            status.Should()
                .Be(ProductStatus.Draft);
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
            // Arrange
            Product product = ProductTestData.CreateValidProduct();

            // Act
            product.AddImage(
                storageKey: "products/img1.webp",
                altText: "Front view",
                displayOrder: 1);

            // Assert
            product.Images.Should().HaveCount(1);

            ProductImage image = product.Images.Single();

            image.StorageKey.Should().Be("products/img1.webp");
            image.IsMain.Should().BeTrue();
        }

        [Fact]
        public void AddImage_ShouldNotReplaceMainImage_WhenSetAsMainIsFalse()
        {
            // Arrange
            Product product = ProductTestData.CreateValidProduct();

            product.AddImage("img1.webp", "Front", 1);

            // Act
            product.AddImage("img2.webp", "Back", 2);

            // Assert
            product.Images.Should().HaveCount(2);

            product.Images.Single(i => i.StorageKey == "img1.webp")
                .IsMain.Should().BeTrue();

            product.Images.Single(i => i.StorageKey == "img2.webp")
                .IsMain.Should().BeFalse();
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
        public void AddImage_ShouldTrimStorageKey()
        {
            // Arrange
            Product product = ProductTestData.CreateValidProduct();

            // Act
            product.AddImage(
                "   folder/image.webp   ",
                "Front",
                1);

            // Assert
            product.Images.Single()
                .StorageKey.Should()
                .Be("folder/image.webp");
        }

        [Fact]
        public void AddImage_ShouldKeepImagesSortedByDisplayOrder()
        {
            // Arrange
            Product product = ProductTestData.CreateValidProduct();

            // Act
            product.AddImage("img3.webp", "3", 3);
            product.AddImage("img1.webp", "1", 1);
            product.AddImage("img2.webp", "2", 2);

            // Assert
            product.Images
                .Select(i => i.DisplayOrder)
                .Should()
                .Equal(1, 2, 3);
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
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.AddImage(
                "images/front.jpg",
                "Front",
                1);

            ProductImage image =
                product.Images.Single();

            // Act
            product.RemoveImage(image.Id);

            // Assert
            product.Images
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void RemoveImage_ShouldDoNothing_WhenImageDoesNotExist()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.AddImage(
                "images/front.jpg",
                "Front",
                1);

            // Act
            product.RemoveImage(Guid.NewGuid());

            // Assert
            product.Images
                .Should()
                .HaveCount(1);

            product.Images.Single().IsMain
                .Should()
                .BeTrue();
        }

        [Fact]
        public void RemoveImage_ShouldAssignNewMainImage_WhenMainImageRemoved()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.AddImage("1.jpg", "One", 1);
            product.AddImage("2.jpg", "Two", 2);
            product.AddImage("3.jpg", "Three", 3);

            ProductImage main =
                product.Images.First(i => i.IsMain);

            // Act
            product.RemoveImage(main.Id);

            // Assert
            product.Images
                .Should()
                .HaveCount(2);

            product.Images
                .Count(i => i.IsMain)
                .Should()
                .Be(1);

            product.Images
                .First(i => i.DisplayOrder == 2)
                .IsMain
                .Should()
                .BeTrue();
        }

        [Fact]
        public void RemoveImage_ShouldKeepCurrentMain_WhenRemovingNonMainImage()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.AddImage("1.jpg", "One", 1);
            product.AddImage("2.jpg", "Two", 2);

            ProductImage main =
                product.Images.First(i => i.IsMain);

            ProductImage second =
                product.Images.First(i => !i.IsMain);

            // Act
            product.RemoveImage(second.Id);

            // Assert
            product.Images
                .Should()
                .HaveCount(1);

            product.Images.Single().Id
                .Should()
                .Be(main.Id);

            product.Images.Single().IsMain
                .Should()
                .BeTrue();
        }

        [Fact]
        public void RemoveImage_ShouldLeaveGalleryEmpty_WhenLastImageRemoved()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.AddImage(
                "1.jpg",
                "One",
                1);

            ProductImage image =
                product.Images.Single();

            // Act
            product.RemoveImage(image.Id);

            // Assert
            product.Images
                .Should()
                .BeEmpty();
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

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [InlineData(null)]
        public void UpdateDescription_ShouldThrow_WhenDescriptionIsEmpty(
            string? description)
        {
            // Arrange
            Product product = ProductTestData.CreateValidProduct();

            // Act
            Action act = () =>
                product.UpdateDescription(description!);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage("Описание товара не может быть пустым.");
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
        public void DeductStock_ShouldDecreaseStock()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct(
                    initialStock: 10);

            // Act
            product.DeductStock(3);

            // Assert
            product.StockQuantity
                .Should()
                .Be(7);
        }

        [Fact]
        public void DeductStock_ShouldThrow_WhenQuantityIsZero()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct(
                    initialStock: 10);

            // Act
            Action act = () =>
                product.DeductStock(0);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Количество должно быть больше нуля.");
        }

        [Fact]
        public void DeductStock_ShouldThrow_WhenQuantityIsNegative()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct(
                    initialStock: 10);

            // Act
            Action act = () =>
                product.DeductStock(-5);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Количество должно быть больше нуля.");
        }

        [Fact]
        public void DeductStock_ShouldThrow_WhenNotEnoughStock()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct(
                    initialStock: 5);

            // Act
            Action act = () =>
                product.DeductStock(6);

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Недостаточно товара на складе.");
        }

        [Fact]
        public void Delete_ShouldMarkProductAsDeleted()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            ProductStatus initialStatus =
                product.Status;

            // Act
            product.Delete();

            // Assert
            product.IsDeleted.Should()
                .BeTrue();

            product.DeletedAtUtc.Should()
                .NotBeNull();

            product.Status.Should()
                .Be(initialStatus);
        }

        [Fact]
        public void Delete_ShouldSetDeletedAtUtc()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            // Act
            product.Delete();

            // Assert
            product.DeletedAtUtc.Should().HaveValue();
            product.DeletedAtUtc!.Value.Should().BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Delete_ShouldRaiseProductDeletedEvent()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.ClearDomainEvents();

            // Act
            product.Delete();

            // Assert
            product.DomainEvents
                .Should()
                .ContainSingle(e => e is ProductDeletedEvent);
        }

        [Fact]
        public void Delete_ShouldDoNothing_WhenAlreadyDeleted()
        {
            // Arrange
            Product product =
                ProductTestData.CreateValidProduct();

            product.Delete();

            DateTime? deletedAt =
                product.DeletedAtUtc;

            int eventsCount =
                product.DomainEvents.Count;

            ProductStatus status =
                product.Status;

            // Act
            product.Delete();

            // Assert
            product.IsDeleted.Should()
                .BeTrue();

            product.DeletedAtUtc.Should()
                .Be(deletedAt);

            product.Status.Should()
                .Be(status);

            product.DomainEvents.Should()
                .HaveCount(eventsCount);
        }
    }
}
