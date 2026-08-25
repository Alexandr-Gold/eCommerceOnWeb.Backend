using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Seed
{
    public static class ProductSeeder
    {
        public static async Task SeedAsync(
            ECommerceDbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            const string sku = "APL-IP16PRO-256";

            if (await context.Products.AnyAsync(
                    p => p.Sku == sku,
                    cancellationToken))
            {
                logger.LogInformation(
                     "Product with SKU {Sku} already exists. Skipped.",
                     sku);

                return;
            }

            Product iphone = Product.Create(
                name: "Apple iPhone 16 Pro 256GB",
                sku: sku,
                gtin: "1234567890123",
                modelNumber: "A3293",
                price: new Money(1499m, "USD"),
                categoryId: SeedIds.SmartphonesCategoryId,
                brandId: SeedIds.AppleBrandId,
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

            logger.LogInformation(
                 "Product {Sku} added to seed.",
                 sku);
        }
    }
}
