using MediatR;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails
{
    public record AddProductImageCommand(
     Guid ProductId,
     string StorageKey,   // Принимаем только уникальный ключ хранилища!
     string AltText,
     int DisplayOrder,
     bool IsMain
 ) : IRequest<Guid>;      // Возвращает ID созданной записи изображения
}
