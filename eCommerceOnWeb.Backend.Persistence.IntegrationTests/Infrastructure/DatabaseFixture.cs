using eCommerceOnWeb.Backend.Application;
using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using eCommerceOnWeb.Backend.Persistence.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Respawn;
using Respawn.Graph;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string TestDatabaseName = "ecommerce_test_db";

    private ServiceProvider? _serviceProvider;
    private Respawner? _respawner;

    public string ConnectionString { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }

    public Guid BrandId { get; private set; }


    // --------------------------------------------------
    // Initialization
    // --------------------------------------------------

    public async Task InitializeAsync()
    {
        ConnectionString = GetConnectionString();

        ValidateDatabaseConnection(ConnectionString);

        // AddPersistence() expects ConnectionStrings:DefaultConnection.
        ConfigurationManager configuration = new ConfigurationManager();

        configuration["ConnectionStrings:DefaultConnection"] =
            ConnectionString;

        ServiceCollection services = new ServiceCollection();

        services.AddLogging();

        services.AddApplication();

        services.AddPersistence(configuration);

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

        try
        {
            // Apply EF Core migrations.
            await using (AsyncServiceScope scope =
                _serviceProvider.CreateAsyncScope())
            {
                ECommerceDbContext context =
                    scope.ServiceProvider
                        .GetRequiredService<ECommerceDbContext>();

                await context.Database.MigrateAsync();
            }

            // Initialize Respawn using the actual PostgreSQL schema.
            await using (NpgsqlConnection connection =
                new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                _respawner = await Respawner.CreateAsync(
                    connection,
                    new RespawnerOptions
                    {
                        DbAdapter = DbAdapter.Postgres,

                        SchemasToInclude =
                        [
                            "public"
                        ],

                        // Keep EF Core migration history.
                        // All application tables are reset.
                        TablesToIgnore =
                        [
                            new Table("public", "__EFMigrationsHistory")
                        ]
                    });
            }

            // Start with a clean database and seed reference data.
            await ResetAsync();
        }
        catch
        {
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;

            throw;
        }
    }


    // --------------------------------------------------
    // Database reset
    // --------------------------------------------------

    public async Task ResetAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException(
                "DatabaseFixture has not been initialized.");
        }

        // Remove data from all application tables.
        // Respawn respects foreign key dependencies.
        await using (NpgsqlConnection connection =
            new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
        }

        // Re-create reference data after every reset.
        await using AsyncServiceScope scope =
            _serviceProvider!.CreateAsyncScope();

        ECommerceDbContext context =
            scope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

        (CategoryId, BrandId) =
            await TestReferenceData.SeedAsync(context);
    }


    // --------------------------------------------------
    // DI scope
    // --------------------------------------------------

    public IServiceScope CreateScope()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "DatabaseFixture has not been initialized.");
        }

        return _serviceProvider.CreateScope();
    }


    // --------------------------------------------------
    // Connection string
    // --------------------------------------------------

    private static string GetConnectionString()
    {
        // First try the current process environment.
        string? connectionString =
            Environment.GetEnvironmentVariable(
                "ECOMMERCE_TEST_DB_CONNECTION");

        // Then try the persistent Windows user environment.
        // This also handles the setx scenario.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                connectionString =
                    Environment.GetEnvironmentVariable(
                        "ECOMMERCE_TEST_DB_CONNECTION",
                        EnvironmentVariableTarget.User);
            }
            catch (PlatformNotSupportedException)
            {
                // User-level environment variables may not be
                // supported on non-Windows platforms.
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ECOMMERCE_TEST_DB_CONNECTION is not configured.");
        }

        return connectionString;
    }


    private static void ValidateDatabaseConnection(
        string connectionString)
    {
        NpgsqlConnectionStringBuilder builder =
            new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.Equals(
                builder.Database,
                TestDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Integration tests must use '{TestDatabaseName}'. " +
                "Connection to another database is prohibited.");
        }
    }


    // --------------------------------------------------
    // Disposal
    // --------------------------------------------------

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;
        }
    }
}


// ======================================================
// Reference data for integration tests
// ======================================================

internal static class TestReferenceData
{
    public static async Task<(Guid CategoryId, Guid BrandId)> SeedAsync(
        ECommerceDbContext context)
    {
        Category category = Category.Create(
            "Electronics",
            "electronics");

        Brand brand = Brand.Create(
            "Apple",
            "apple");

        context.Categories.Add(category);
        context.Brands.Add(brand);

        await context.SaveChangesAsync();

        return (category.Id, brand.Id);
    }
}
