using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

public sealed class PostgresTestDatabaseFixture : IAsyncLifetime
{
    private TestServiceProvider? _serviceProvider;
    private Respawner? _respawner;

    public string ConnectionString { get; private set; } = string.Empty;

    internal TestSeedData SeedData { get; private set; } = null!;

    internal TestServiceProvider ServiceProvider =>
        _serviceProvider ?? throw new InvalidOperationException(
            "Поставщик услуг тестирования не инициализирован.");

    public async Task InitializeAsync()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<PostgresTestDatabaseFixture>()
            .AddEnvironmentVariables()
            .Build();

        ConnectionString = configuration.GetConnectionString(
            "DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection не настроено.");

        ValidateTestDatabaseConnectionString(ConnectionString);

        _serviceProvider = new TestServiceProvider(configuration);

        await MigrateDatabaseAsync();
        SeedData = await SeedDatabaseAsync();
        await CreateRespawnerAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        EnsureInitialized();

        if (_respawner is null)
            throw new InvalidOperationException(
                "Respawner не инициализировано.");

        await using NpgsqlConnection connection = new NpgsqlConnection(
            ConnectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);

        SeedData = await SeedDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    private async Task MigrateDatabaseAsync()
    {
        await using AsyncServiceScope scope = ServiceProvider.CreateScope();
        ECommerceDbContext context = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        await context.Database.MigrateAsync();
    }

    private async Task<TestSeedData> SeedDatabaseAsync()
    {
        await using AsyncServiceScope scope = ServiceProvider.CreateScope();
        ECommerceDbContext context = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        return await TestDataSeeder.SeedAsync(context);
    }

    private async Task CreateRespawnerAsync()
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(
            ConnectionString);
        await connection.OpenAsync();

        _respawner = await RespawnCheckpointFactory.CreateAsync(connection);
    }

    private static void ValidateTestDatabaseConnectionString(
        string connectionString)
    {
        NpgsqlConnectionStringBuilder builder
            = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException(
                "Строка подключения для проверки должна содержать имя " +
                "базы данных.");
        }

        if (!builder.Database.Contains(
            "test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Отказ от выполнения интеграционных тестов с " +
                $"использованием базы данных " +
                $"'{builder.Database}'. " +
                "Используйте отдельную тестовую базу данных.");
        }
    }

    private void EnsureInitialized()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "Тестовое окружение не инициализировано.");
        }
    }
}