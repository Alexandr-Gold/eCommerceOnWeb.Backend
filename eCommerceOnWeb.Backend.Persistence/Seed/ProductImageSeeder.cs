using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Seed
{
    public static class ProductImageSeeder
    {
        private sealed record ProductImageSeedData(
            string ProductSku,
            string StorageKey,
            string AltText,
            int DisplayOrder,
            bool IsMain);


        private static readonly ProductImageSeedData[] Images =
        {
            new(
                "APL-IP16PRO-256",
                "products/apple-iphone16/front-back-desert.jpg",
                "Apple iPhone 16 Pro Desert",
                1,
                true),

            new(
                "APL-IP16PRO-256",
                "products/apple-iphone16/back-desert.jpg",
                "Apple iPhone 16 Pro Back Desert",
                2,
                false),

            new(
                "SAM-S25ULTRA-512",
                "products/samsung-g-s25-ultra/front-back-titanium-black.jpg",
                "Samsung Galaxy S25 Ultra Titanium Black",
                1,
                true),

            new(
                "SAM-S25ULTRA-512",
                "products/samsung-g-s25-ultra/back-titanium-black.jpg",
                "Samsung Galaxy S25 Ultra Back",
                2,
                false)
        };

        public static async Task SeedAsync(
            ECommerceDbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            foreach (ProductImageSeedData imageData in Images)
            {
                Product? product = await context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(
                        p => p.Sku == imageData.ProductSku,
                        cancellationToken);

                if (product is null)
                {
                    logger.LogWarning(
                        "Product with SKU {Sku} not found. Image {Image} skipped.",
                        imageData.ProductSku,
                        imageData.StorageKey);

                    continue;
                }

                bool imageExists = product.Images.Any(i =>
                    i.StorageKey.Equals(
                        imageData.StorageKey,
                        StringComparison.OrdinalIgnoreCase));


                if (imageExists)
                {
                    logger.LogInformation(
                        "Image {StorageKey} already exists. Skipped.",
                        imageData.StorageKey);

                    continue;
                }

                product.AddImage(
                    imageData.StorageKey,
                    imageData.AltText,
                    imageData.DisplayOrder,
                    imageData.IsMain);


                logger.LogInformation(
                    "Image {StorageKey} added to product {Sku}.",
                    imageData.StorageKey,
                    imageData.ProductSku);
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}