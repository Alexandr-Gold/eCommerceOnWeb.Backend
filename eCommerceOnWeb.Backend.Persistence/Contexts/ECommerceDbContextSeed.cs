using eCommerceOnWeb.Backend.Persistence.Seed;

namespace eCommerceOnWeb.Backend.Persistence.Contexts
{
    public static class ECommerceDbContextSeed
    {
        public static async Task SeedAsync(
            ECommerceDbContext context,
            CancellationToken cancellationToken = default)
        {
            await ProductSeeder.SeedAsync(
                context,
                cancellationToken);

            await context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
