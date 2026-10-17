using eCommerceOnWeb.Backend.Application.Common.Behaviors;
using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using FluentValidation;
using MediatR;
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

            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
