namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.GetProductDetails
{
    // Главное DTO ответа
    public record ProductDetailsDto(
    Guid Id,
    string Name,
    string Sku,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    string Description,
    Guid CategoryId,
    Guid BrandId,
    IReadOnlyCollection<ProductImageDto> Images,
    IReadOnlyDictionary<string, object> Attributes
    );
}
