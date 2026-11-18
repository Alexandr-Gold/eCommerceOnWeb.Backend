using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;

using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

internal static class TestDataSeeder
{
    public static async Task<TestSeedData> SeedAsync(
        ECommerceDbContext context,
        CancellationToken cancellationToken = default)
    {
        Category category = await EnsureCategoryAsync(context, cancellationToken);
        Brand brand = await EnsureBrandAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new TestSeedData(
            category.Id,
            brand.Id);
    }

    private static async Task<Category> EnsureCategoryAsync(
        ECommerceDbContext context,
        CancellationToken cancellationToken)
    {
        Category? existingCategory = await context.Categories
            .FirstOrDefaultAsync(x => x.Slug == "electronics", cancellationToken);

        if (existingCategory is not null)
            return existingCategory;

        Category category = Category.Create(
            "Electronics",
            "electronics");

        context.Categories.Add(category);

        return category;
    }

    private static async Task<Brand> EnsureBrandAsync(
        ECommerceDbContext context,
        CancellationToken cancellationToken)
    {
        Brand? existingBrand = await context.Brands
            .FirstOrDefaultAsync(x => x.Slug == "apple", cancellationToken);

        if (existingBrand is not null)
            return existingBrand;

        Brand brand = Brand.Create(
            "Apple",
            "apple");

        context.Brands.Add(brand);

        return brand;
    }
}