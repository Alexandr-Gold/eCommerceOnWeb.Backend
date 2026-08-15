using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using MediatR;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler
        : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IECommerceDbContext _context;

        public CreateProductCommandHandler(IECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            Money price = Money.Create(
                request.PriceAmount,
                request.PriceCurrency);

            Dimensions dimensions = new Dimensions(
                request.DimWidthCm,
                request.DimHeightCm,
                request.DimDepthCm,
                request.DimWeightKg);

            dimensions.EnsureValidForShipping();

            Product product = Product.Create(
                request.Name,
                request.Sku,
                request.Gtin,
                request.ModelNumber,
                price,
                request.CategoryId,
                request.BrandId,
                request.Description,
                request.WarrantyMonths,
                dimensions,
                request.InitialStock);

            foreach (ProductAttributeDto attribute in request.Attributes)
            {
                object value = attribute.Value.ValueKind switch
                {
                    JsonValueKind.String => attribute.Value.GetString()!,
                    JsonValueKind.Number when attribute.Value.TryGetInt64(out long l) => l,
                    JsonValueKind.Number => attribute.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => throw new DomainException("Атрибут не может иметь значение null."),
                    _ => throw new InvalidOperationException(
                             "Неподдерживаемый тип атрибута.")
                };

                product.SetAttribute(attribute.Key, value);

                //product.SetAttribute(attribute.Key, attribute.Value);
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}