using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    public record GetProductDetailsQuery(Guid Id) : IRequest<ProductDetailsDto?>;
}
