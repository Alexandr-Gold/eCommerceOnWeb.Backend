using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    public record GetProductDetailsQuery(Guid Id) : IRequest<Result<ProductDetailsDto>>;
}
