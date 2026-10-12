using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Seed
{
    public static class BrandSeeder
    {
        public static async Task<Brand> SeedAsync(
            ECommerceDbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            const string slug = "apple";

            Brand? brand = await context.Brands
                .FirstOrDefaultAsync(
                    b => b.Slug == slug,
                    cancellationToken);

            if (brand != null)
            {
                logger.LogInformation(
                    "Brand with slug {Slug} already exists.",
                    slug);

                return brand;
            }

            Brand apple = Brand.Create(
                name: "Apple",
                slug: slug);

            context.Brands.Add(apple);


            logger.LogInformation(
                "Brand {Slug} added to seed.",
                slug);

            return apple;
        }
    }
}