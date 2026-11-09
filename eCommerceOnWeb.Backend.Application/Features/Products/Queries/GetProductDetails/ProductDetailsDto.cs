using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails;

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
ProductStatus Status,
IReadOnlyCollection<ProductImageDto> Images,
IReadOnlyDictionary<string, object> Attributes
);
