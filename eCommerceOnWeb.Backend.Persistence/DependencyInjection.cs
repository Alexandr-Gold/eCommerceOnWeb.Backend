using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using eCommerceOnWeb.Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerceOnWeb.Backend.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(
     this IServiceCollection services,
     IConfiguration configuration)
        {
            services.AddDbContext<ECommerceDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IECommerceDbContext>(
                provider => provider.GetRequiredService<ECommerceDbContext>());

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            return services;
        }
    }
}
