namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    // DTO картинки с URL
    public record ProductImageDto(
     Guid Id,
     string Url,          // Полноценная ссылка (например, https://s3.../photo.jpg)
     string StorageKey,   // Сам ключ (полезно фронтенду для удаления/изменения)
     string AltText,
     int DisplayOrder,
     bool IsMain
    );
}
