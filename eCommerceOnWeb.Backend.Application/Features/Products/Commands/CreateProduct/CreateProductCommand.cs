using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Команда MediatR для создания новой карточки товара электроники.
    /// Возвращает Guid созданного продукта.
    /// </summary>
    // плоская структура для передачи одной характеристики из Swagger/фронтенда
    public sealed record ProductAttributeDto(
    string Key,
    JsonElement Value);

    /// <summary>
    /// Команда для создания карточки товара электроники
    /// с поддержкой дополнительных атрибутов товара, хранящихся в JSONB.
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
        List<ProductAttributeDto> Attributes) : IRequest<Result<Guid>>;
}

