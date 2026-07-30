using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Application.Common.Pagination;
using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductList
{
    public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PaginatedResult<ProductListDto>>>
    {
        private readonly IECommerceDbContext _context;
        private readonly IStorageService _storageService;

        public GetProductsQueryHandler(IECommerceDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<Result<PaginatedResult<ProductListDto>>> Handle(
            GetProductsQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Базовый запрос с включением изображений (нужны для получения главного)
            IQueryable<Domain.Aggregates.ProductAggregate.Product> query = _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .AsQueryable();

            // 2. Применяем фильтры
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string search = request.SearchTerm.Trim();
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Sku.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.BrandId.HasValue)
                query = query.Where(p => p.BrandId == request.BrandId.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price.Amount >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price.Amount <= request.MaxPrice.Value);

            // 3. Подсчёт общего количества (до пагинации)
            int totalCount = await query.CountAsync(cancellationToken);

            // 4. Пагинация
            List<Domain.Aggregates.ProductAggregate.Product> products = await query
                .OrderBy(p => p.Name) // или по Id, или по дате создания – выбери своё
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // 5. Маппинг в DTO
            List<ProductListDto> items = products.Select(product =>
            {
                // Находим главное изображение (IsMain == true) или первое по порядку
                Domain.Aggregates.ProductAggregate.ProductImage? mainImage = product.Images.FirstOrDefault(img => img.IsMain)
                                ?? product.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault();

                string? mainImageUrl = mainImage != null
                    ? _storageService.GetAbsoluteUrl(mainImage.StorageKey)
                    : null;

                return new ProductListDto(
                          product.Id,
                          product.Name ?? string.Empty,
                          product.Sku ?? string.Empty,
                          product.Price.Amount,
                          product.Price.Currency ?? "RUB",
                          product.StockQuantity,
                          product.Description ?? string.Empty,   // сюда
                          mainImageUrl
                );
            }).ToList();

            // 6. Формируем пагинированный ответ
            PaginatedResult<ProductListDto> result = new PaginatedResult<ProductListDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            // 7. Возвращаем успешный Result
            return Result<PaginatedResult<ProductListDto>>.Success(result);
        }
    }
}
