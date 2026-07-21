using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public class ProductImage : BaseEntity
    {
        public string StorageKey { get; private set; } = string.Empty;
        public string AltText { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; } // Для сортировки в галерее
        public bool IsMain { get; internal set; }    // Управляется только через агрегат Product

        private ProductImage() { }

        public ProductImage(string storagekey, string altText, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(storagekey)) throw new DomainException("Storage Key изображения обязателен.");

            StorageKey = storagekey;
            AltText = altText;
            DisplayOrder = displayOrder;
            IsMain = false;
        }

        public void UpdateDetails(string altText, int displayOrder)
        {
            if (displayOrder < 0)
                throw new DomainException("Display Order не может быть отрицательным.");

            if (string.IsNullOrWhiteSpace(altText))
                throw new DomainException("Alt Text не может быть пустым.");

            AltText = altText.Trim();
            DisplayOrder = displayOrder;
        }
    }
}
