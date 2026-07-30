using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProducts
{
    // Обертка для ответа пагинации
    public sealed record PaginatedList<T>(IReadOnlyCollection<T> Items, int PageIndex, int TotalPages, int TotalCount)
    {
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    // Плоский DTO товара для каталога
    public sealed record ProductDto(
        Guid Id,
        string Name,
        string Sku,
        string? Gtin,
        string ModelNumber,
        decimal PriceAmount,
        string PriceCurrency,
        int StockQuantity,
        Guid CategoryId,
        Guid BrandId,
        string Description,
        IReadOnlyDictionary<string, object> Specifications); // ДОБАВЛЕНО ПОЛЕ ДЛЯ JSONB ХАРАКТЕРИСТИК;

    // Сам Query-запрос (record MediatR)
    public sealed record GetProductsWithPaginationQuery(
        Guid? CategoryId,
        Guid? BrandId,
        string? SearchTerm,
        int PageIndex = 1,
        int PageSize = 10) : IRequest<PaginatedList<ProductDto>>;
}
