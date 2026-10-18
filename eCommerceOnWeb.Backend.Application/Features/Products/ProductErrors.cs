using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Application.Features.Products
{
    public static class ProductErrors
    {
        public static Error NotFound(Guid id) =>
            new(
                "Products.NotFound",
                $"Товар с идентификатором '{id}' не найден.");

        public static Error SkuAlreadyExists(string sku) =>
             new(
                 "Products.SkuAlreadyExists",
                 $"Товар с SKU '{sku}' уже существует.");

        public static Error GtinAlreadyExists(string gtin) =>
            new(
                "Products.GtinAlreadyExists",
                $"Товар с GTIN '{gtin}' уже существует.");
    }
}
