using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    public static class ProductErrors
    {
        public static Error NotFound(Guid id) =>
            new("Products.NotFound", $"Товар с идентификатором {id} не найден.");
    }
}
