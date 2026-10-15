using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerceOnWeb.Backend.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    typeof(DependencyInjection).Assembly);
            });

            services.AddValidatorsFromAssembly(
                typeof(CreateProductCommandValidator).Assembly);

            return services;
        }
    }
}
