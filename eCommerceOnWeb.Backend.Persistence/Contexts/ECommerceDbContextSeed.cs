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

            await context.SaveChangesAsync(
                cancellationToken);

            logger.LogInformation(
                "Database seeding completed.");
        }
    }

    //public static class ECommerceDbContextSeed
    //{
    //    public static async Task SeedAsync(
    //        ECommerceDbContext context,
    //        CancellationToken cancellationToken = default)
    //    {
    //        await ProductSeeder.SeedAsync(
    //            context,
    //            cancellationToken);

    //        await context.SaveChangesAsync(
    //            cancellationToken);
    //    }
    //}
}
