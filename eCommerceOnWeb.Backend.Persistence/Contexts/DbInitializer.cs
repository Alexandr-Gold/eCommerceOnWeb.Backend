using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Persistence.Contexts;
public static class DbInitializer
{
    public static async Task SeedAsync(ECommerceDbContext context)
    {
        // 1. Проверяем наличие незавершенных миграций
        //if ((await context.Database.GetPendingMigrationsAsync()).Any())
        //{
        //    await context.Database.MigrateAsync();
        //}

        //// 2. Если в базе уже есть товары, выходим (защита от дублирования)
        //if (await context.Products.AnyAsync())
        //{
        //    return;
        //}

        // 3. Генерируем временные Guid для Категории и Брендов.
        Guid fakeElectronicsCategoryId = Guid.NewGuid();
        Guid fakeAppleBrandId = Guid.NewGuid();

        // 4. Создаем экземпляры структур данных / Value Objects
        // (Замените 'new Money' и 'new Dimensions' на ваши реальные конструкторы, если имена параметров отличаются)
        Money productPrice = new Money(119990.00m, "RUB");
        Dimensions productDimensions = new Dimensions(7.06m, 14.66m, 0.83m, 0.187m);

        // 5. Создаем товар строго по сигнатуре вашего конструктора Product
        Product phone = new Product(
            name: "Смартфон iPhone 15 Pro",
            sku: "AAPL-IPH15P-128BK",
            gtin: "195949033444",
            modelNumber: "A3102",
            price: productPrice,
            categoryId: fakeElectronicsCategoryId,
            brandId: fakeAppleBrandId,
            warrantyMonths: 12,
            shippingDimensions: productDimensions,
            initialStock: 15
        );

        // 6.Добавляем изображения через ВАШ БИЗНЕС-МЕТОД (Инвариант галереи)
        // ВАЖНО: В методе AddImage у вас первым параметром идет 'url'. Передаем туда наш StorageKey.
        phone.AddImage(
            url: "products/iphone15-main.png",
            altText: "iPhone 15 Pro вид спереди",
            displayOrder: 0,
            setAsMain: true
        );

        phone.AddImage(
            url: "products/iphone15-back.png",
            altText: "iPhone 15 Pro вид сзади",
            displayOrder: 1,
            setAsMain: false
        );

        // 7. Сохраняем товар в контекст EF Core и отправляем в PostgreSQL
        await context.Products.AddAsync(phone);
        await context.SaveChangesAsync();
    }
}