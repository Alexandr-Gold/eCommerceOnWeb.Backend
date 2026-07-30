namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductList
{
    // Облегченное DTO
    public record ProductListDto(
     Guid Id,
     string Name,
     string Sku,
     decimal PriceAmount,
     string PriceCurrency,
     int StockQuantity,
     string Description,
     string? MainImageUrl // только главное изображение для списка
 );
}
