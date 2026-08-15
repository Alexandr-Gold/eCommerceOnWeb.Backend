using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.GetProductDetails
{
    public record GetProductDetailsQuery(Guid Id) : IRequest<Result<ProductDetailsDto>>;
}
