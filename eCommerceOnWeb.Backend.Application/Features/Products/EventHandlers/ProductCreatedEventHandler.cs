using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Application.Features.Products.EventHandlers
{
    public class ProductCreatedEventHandler
        : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedEventHandler> _logger;

        public ProductCreatedEventHandler(
            ILogger<ProductCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(
            ProductCreatedEvent notification,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Product created. Id: {ProductId}, SKU: {Sku}, Price: {Price}",
                notification.ProductId,
                notification.Sku,
                notification.Price.Amount);

            return Task.CompletedTask;
        }
    }
}
