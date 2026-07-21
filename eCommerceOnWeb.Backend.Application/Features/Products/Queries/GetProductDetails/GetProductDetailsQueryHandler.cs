using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, ProductDetailsDto?>
{
    private readonly IECommerceDbContext _context;
    private readonly IStorageService _storageService;

    public GetProductDetailsQueryHandler(IECommerceDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ProductDetailsDto?> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        // 1. Извлекаем товар из БД (БЕЗ вызова сторонних сервисов внутри LINQ)
        eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Product? product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return null;
        }

        // 2. Вся работа с C#-методами (генерация ссылок) происходит здесь — в оперативной памяти (In-Memory)
        List<ProductImageDto> imagesDto = product.Images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => new ProductImageDto(
                img.Id,
                _storageService.GetAbsoluteUrl(img.StorageKey), // ⚡ Теперь это отработает идеально!
                img.StorageKey,
                img.AltText ?? string.Empty,
                img.DisplayOrder,
                img.IsMain
            ))
            .ToList();

        // 3. Возвращаем готовый DTO
        return new ProductDetailsDto(
            product.Id,
            product.Name ?? string.Empty,
            product.Sku ?? string.Empty,
            product.Price.Amount,
            product.Price.Currency ?? "RUB",
            product.StockQuantity,
            product.CategoryId,
            product.BrandId,
            imagesDto
        );
    }
}