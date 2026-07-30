using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Persistence.Contexts
{
    public static class ECommerceDbContextSeed
    {
        public static async Task SeedAsync(ECommerceDbContext context)
        {
            //context.Products.RemoveRange(context.Products);
            //await context.SaveChangesAsync();

            //Если товары уже существуют -ничего не делаем
            if (await context.Products.AnyAsync())
                return;

            Guid categoryId = Guid.NewGuid();
            Guid brandId = Guid.NewGuid();

            Product iphone = Product.Create(
                name: "Apple iPhone 16 Pro 256GB",
                sku: "APL-IP16PRO-256",
                gtin: "1234567890123",
                modelNumber: "A3293",
                price: new Money(1499m, "USD"),
                categoryId: categoryId,
                brandId: brandId,
                description: "Флагманский смартфон Apple с OLED-дисплеем.",
                warrantyMonths: 24,
                shippingDimensions: new Dimensions(
                     WidthCm: 8,
                     HeightCm: 16,
                     DepthCm: 2,
                     WeightKg: 0.35m),
                initialStock: 15);

            iphone.AddImage(
                "products/iphone16/front.webp",
                "Apple iPhone 16 Pro",
                1,
                true);

            context.Products.Add(iphone);

            await context.SaveChangesAsync();
        }
    }
}
