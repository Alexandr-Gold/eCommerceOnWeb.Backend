using eCommerceOnWeb.Backend.Application;
using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

internal sealed class TestServiceProvider : IAsyncDisposable
{
    private readonly ServiceProvider _root;

    public TestServiceProvider(IConfiguration configuration)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton(configuration);

        services.AddSingleton<IStorageService, TestStorageService>();

        services.AddApplication();
        services.AddPersistence(configuration);

        _root = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    public AsyncServiceScope CreateScope() => _root.CreateAsyncScope();

    public ValueTask DisposeAsync() => _root.DisposeAsync();
}