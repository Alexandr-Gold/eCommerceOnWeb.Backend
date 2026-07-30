using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.CreateProduct
{
    /// <summary>
    /// Команда MediatR для создания новой карточки товара электроники.
    /// Возвращает Guid созданного продукта.
    /// </summary>
    // плоская структура для передачи одной характеристики из Swagger/фронтенда
    public sealed record ProductSpecificationDto(string Key, object Value);

    /// <summary>
    /// Обновленная команда для создания карточки товара электроники с поддержкой JSONB спецификаций.
    /// </summary>
    public sealed record CreateProductCommand(
        string Name,
        string Description,
        string Sku,
        string? Gtin,
        string ModelNumber,
        decimal PriceAmount,
        string PriceCurrency,
        Guid CategoryId,
        Guid BrandId,
        int WarrantyMonths,
        decimal DimWidthCm,
        decimal DimHeightCm,
        decimal DimDepthCm,
        decimal DimWeightKg,
        int InitialStock,
        List<ProductSpecificationDto> Specifications) : IRequest<Guid>;
}

