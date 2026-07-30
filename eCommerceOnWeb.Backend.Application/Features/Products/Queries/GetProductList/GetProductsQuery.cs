using eCommerceOnWeb.Backend.Application.Common.Pagination;
using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductList
{
    public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Description = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<Result<PaginatedResult<ProductListDto>>>;
}
