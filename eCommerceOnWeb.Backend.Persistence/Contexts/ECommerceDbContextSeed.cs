using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using eCommerceOnWeb.Backend.Persistence.Seed;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Contexts
{
    public static class ECommerceDbContextSeed
    {
        public static async Task SeedAsync(
            ECommerceDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken = default)
        {
            ILogger logger =
                loggerFactory.CreateLogger("DatabaseSeeder");

            logger.LogInformation(
                "Database seeding started.");

            // ---------- Справочники ----------
            Category category =
                 await CategorySeeder.SeedAsync(
                     context,
                     logger,
                     cancellationToken);

            Brand brand =
                 await BrandSeeder.SeedAsync(
                     context,
                     logger,
                     cancellationToken);

            // Сначала сохраняем справочники
            await context.SaveChangesAsync(
                cancellationToken);

            // ---------- Товары ----------
            await ProductSeeder.SeedAsync(
                 context,
                 logger,
                 category,
                 brand,
                 cancellationToken);

            // Затем сохраняем товары и изображения
            await ProductImageSeeder.SeedAsync(
                context,
                logger,
                cancellationToken);

            await context.SaveChangesAsync(
                cancellationToken);

            logger.LogInformation(
                "Database seeding completed.");
        }
    }
}




