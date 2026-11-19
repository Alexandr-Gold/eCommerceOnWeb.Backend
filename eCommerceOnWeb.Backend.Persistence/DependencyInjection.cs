using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using eCommerceOnWeb.Backend.Persistence.Common;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerceOnWeb.Backend.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ECommerceDbContext>(
            (serviceProvider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));

            // для интеграционных тестов
            options.AddInterceptors(
                serviceProvider.GetServices<IInterceptor>());
        });

        services.AddScoped<IECommerceDbContext>(
            provider => provider.GetRequiredService<ECommerceDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        services.AddScoped<IDbConstraintViolationDetector,
            PostgresConstraintViolationDetector>();

        return services;
    }
}
