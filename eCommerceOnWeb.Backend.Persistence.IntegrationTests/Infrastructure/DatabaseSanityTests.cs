using eCommerceOnWeb.Backend.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

[Collection(PostgresTestCollection.Name)]
public sealed class DatabaseSanityTests
{
    private readonly PostgresTestDatabaseFixture _fixture;

    public DatabaseSanityTests(PostgresTestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ConnectionString_ShouldPointToTestDatabase()
    {
        _fixture.ConnectionString.Should().NotBeNullOrWhiteSpace();

        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(_fixture.ConnectionString);

        builder.Database.Should().NotBeNullOrWhiteSpace();
        builder.Database.Should().Be("ecommerce_test_db");
    }

    [Fact]
    public async Task SeedData_ShouldCreateCategoryAndBrand()
    {
        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ECommerceDbContext context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        int categories = await context.Categories.CountAsync();
        int brands = await context.Brands.CountAsync();

        categories.Should().BeGreaterThan(0);
        brands.Should().BeGreaterThan(0);

        _fixture.SeedData.CategoryId.Should().NotBeEmpty();
        _fixture.SeedData.BrandId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ResetDatabase_ShouldRestoreSeedData()
    {
        await _fixture.ResetDatabaseAsync();

        await using AsyncServiceScope scope = _fixture.ServiceProvider.CreateScope();
        ECommerceDbContext context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        bool categoryExists = await context.Categories
            .AnyAsync(x => x.Id == _fixture.SeedData.CategoryId);

        bool brandExists = await context.Brands
            .AnyAsync(x => x.Id == _fixture.SeedData.BrandId);

        categoryExists.Should().BeTrue();
        brandExists.Should().BeTrue();
    }
}