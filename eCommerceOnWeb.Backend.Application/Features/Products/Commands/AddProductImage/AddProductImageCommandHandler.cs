using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.GetProductDetails
{
    public sealed class AddProductImageCommandHandler
    : IRequestHandler<AddProductImageCommand, Guid>
    {
        private readonly IECommerceDbContext _context;

        public AddProductImageCommandHandler(
            IECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(
            AddProductImageCommand request,
            CancellationToken cancellationToken)
        {
            Product? product = await _context.Products
                .FindAsync(
                    new object[] { request.ProductId },
                    cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    nameof(Product),
                    request.ProductId);
            }

            product.AddImage(
                request.StorageKey,
                request.AltText,
                request.DisplayOrder,
                request.IsMain);

            await _context.SaveChangesAsync(cancellationToken);

            ProductImage image = product.Images
                .First(i =>
                    i.StorageKey == request.StorageKey &&
                    i.DisplayOrder == request.DisplayOrder);

            return image.Id;
        }
    }
}
