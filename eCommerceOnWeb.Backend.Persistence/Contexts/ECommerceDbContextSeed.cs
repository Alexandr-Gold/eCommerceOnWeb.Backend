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

            await ProductSeeder.SeedAsync(
                context,
                logger,
                cancellationToken);

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




