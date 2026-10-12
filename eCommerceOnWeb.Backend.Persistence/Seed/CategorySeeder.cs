using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Seed
{
    public static class CategorySeeder
    {
        public static async Task<Category> SeedAsync(
            ECommerceDbContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            const string slug = "smartphones";

            Category? category = await context.Categories
                .FirstOrDefaultAsync(
                    c => c.Slug == slug,
                    cancellationToken);

            if (category != null)
            {
                logger.LogInformation(
                    "Category with slug {Slug} already exists.",
                    slug);

                return category;
            }

            Category smartphones = Category.Create(
                name: "Smartphones",
                slug: slug);

            context.Categories.Add(smartphones);

            logger.LogInformation(
                "Category {Slug} added to seed.",
                slug);

            return smartphones;
        }
    }
}