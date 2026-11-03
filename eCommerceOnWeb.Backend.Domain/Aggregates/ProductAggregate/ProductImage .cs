using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public class ProductImage : BaseEntity
    {
        public string StorageKey { get; private set; } = string.Empty;
        public string AltText { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; } // Для сортировки в галерее

        // Управляется только агрегатом Product,
        // чтобы в галерее всегда существовало не более одного главного изображения.
        public bool IsMain { get; internal set; }

        // EF Core использует этот конструктор при материализации сущности.
        // Создание ProductImage в доменной модели должно выполняться
        // только через публичный конструктор с проверкой инвариантов.
        private ProductImage()
        {
        }

        public ProductImage(
            string storageKey,
            string altText,
            int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new DomainException(
                    "Storage Key изображения обязателен.");

            if (string.IsNullOrWhiteSpace(altText))
                throw new DomainException(
                    "Alt Text не может быть пустым.");

            if (displayOrder < 0)
                throw new DomainException(
                    "Display Order не может быть отрицательным.");

            StorageKey = storageKey.Trim();
            AltText = altText.Trim();
            DisplayOrder = displayOrder;
            IsMain = false;
        }

        public void UpdateDetails(
            string altText,
            int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(altText))
                throw new DomainException(
                    "Alt Text не может быть пустым.");

            if (displayOrder < 0)
                throw new DomainException(
                    "Display Order не может быть отрицательным.");

            AltText = altText.Trim();
            DisplayOrder = displayOrder;
        }
    }
}
