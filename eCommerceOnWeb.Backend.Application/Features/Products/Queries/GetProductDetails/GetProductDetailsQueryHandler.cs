using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{

    // 1. Меняем TResponse в интерфейсе на Result<ProductDetailsDto>
    public sealed class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, Result<ProductDetailsDto>>
    {
        private readonly IECommerceDbContext _context;
        private readonly IStorageService _storageService;

        public GetProductDetailsQueryHandler(IECommerceDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        // 2. Меняем тип возвращаемого значения в сигнатуре метода Handle
        public async Task<Result<ProductDetailsDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            // Извлекаем товар из БД (In-Memory логика остается)
            Domain.Aggregates.ProductAggregate.Product? product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            // 3. Вместо return null возвращаем типизированную ошибку сбоя бизнес-логики
            if (product == null)
            {
                return Result<ProductDetailsDto>.Failure(ProductErrors.NotFound(request.Id));
            }

            // Вся работа со ссылками в памяти остается БЕЗ изменений
            List<ProductImageDto> imagesDto = product.Images
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new ProductImageDto(
                    img.Id,
                    _storageService.GetAbsoluteUrl(img.StorageKey),
                    img.StorageKey,
                    img.AltText ?? string.Empty,
                    img.DisplayOrder,
                    img.IsMain
                ))
                .ToList();

            // создать ProductAttributeDto в GetProductDetails
            // Получаем Атрибуты из БД маппим в ДТО и возвращаем в Лист
            //List<ProductImageDto> imagesDto = product.Atributes
            //   .OrderBy(img => img.DisplayOrder)
            //   .Select(img => new ProductImageDto(
            //       img.Id,
            //       _storageService.GetAbsoluteUrl(img.StorageKey),
            //       img.StorageKey,
            //       img.AltText ?? string.Empty,
            //       img.DisplayOrder,
            //       img.IsMain
            //   ))
            //   .ToList();

            // 4. Формируем и сразу возвращаем DTO (без создания лишней переменной)
            ProductDetailsDto dto = new ProductDetailsDto(
                product.Id,
                product.Name ?? string.Empty,
                product.Sku ?? string.Empty,
                product.Price.Amount,
                product.Price.Currency ?? "RUB",
                product.StockQuantity,
                product.Description ?? string.Empty,
                product.CategoryId,
                product.BrandId,
                imagesDto
            // Add attributesDto
            );
            return Result<ProductDetailsDto>.Success(dto);
        }
    }
}