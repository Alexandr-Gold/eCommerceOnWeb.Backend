namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    // Главное DTO ответа
    public record ProductDetailsDto(
    Guid Id,
    string Name,
    string Sku,
    decimal PriceAmount,     // Убедитесь, что это поле есть
    string PriceCurrency,   // Убедитесь, что это поле есть
    int StockQuantity,
    Guid CategoryId,
    Guid BrandId,
    IReadOnlyCollection<ProductImageDto> Images
);
}
