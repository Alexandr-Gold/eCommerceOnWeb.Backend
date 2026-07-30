using eCommerceOnWeb.Backend.Application.Features.Products.Specifications;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProducts
{
    public sealed class GetProductsWithPaginationQueryHandler
     : IRequestHandler<GetProductsWithPaginationQuery, PaginatedList<ProductDto>>
    {
        private readonly IReadRepository<Product> _product;

        public GetProductsWithPaginationQueryHandler(IReadRepository<Product> product)
        {
            _product = product;
        }

        public async Task<PaginatedList<ProductDto>> Handle(
            GetProductsWithPaginationQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Считаем ОБЩЕЕ количество товаров по фильтрам (пагинация выключена: isPagingEnabled = false)
            // Теперь мы используем ТОЛЬКО ProductFilterSpec и у нас нет ошибки CS0246
            ProductFilterSpec countSpec = new ProductFilterSpec(
                categoryId: request.CategoryId,
                brandId: request.BrandId,
                searchTerm: request.SearchTerm,
                isPagingEnabled: false); // Сработают только блоки Where, без Skip и Take

            int totalCount = await _product.CountAsync(countSpec, cancellationToken);

            // 2. Получаем конкретную страницу данных (пагинация включена по умолчанию)
            ProductFilterSpec filterSpec = new ProductFilterSpec(
                categoryId: request.CategoryId,
                brandId: request.BrandId,
                searchTerm: request.SearchTerm,
                pageIndex: request.PageIndex,
                pageSize: request.PageSize);

            List<Product> products = await _product.ListAsync(filterSpec, cancellationToken);
            // 3. Маппим доменные объекты в плоские DTO с учетом словаря Specifications
            List<ProductDto> productDtos = products.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Sku,
                p.Gtin,
                p.ModelNumber,
                p.Price.Amount,
                p.Price.Currency,
                p.StockQuantity,
                p.CategoryId,
                p.BrandId,
                p.Description,
                p.Specifications // НАПРЯМУЮ ЗАБИРАЕМ СЛОВАРЬ ИЗ JSONB КОЛОНКИ БЕЗ JOIN-ов
            )).ToList();

            // 4. Расчет метаданных пагинации
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            totalPages = totalPages == 0 ? 1 : totalPages;

            return new PaginatedList<ProductDto>(productDtos, request.PageIndex, totalPages, totalCount);
        }
    }
}
