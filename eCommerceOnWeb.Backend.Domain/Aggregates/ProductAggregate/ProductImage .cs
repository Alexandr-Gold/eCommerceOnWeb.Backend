using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public class ProductImage : BaseEntity
    {
        public string Url { get; private set; } = string.Empty;
        public string AltText { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; } // Для сортировки в галерее
        public bool IsMain { get; internal set; }    // Управляется только через агрегат Product

        private ProductImage() { }

        public ProductImage(string url, string altText, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new DomainException("URL изображения обязателен.");

            Url = url;
            AltText = altText;
            DisplayOrder = displayOrder;
            IsMain = false;
        }

        public void UpdateDetails(string altText, int displayOrder)
        {
            AltText = altText;
            DisplayOrder = displayOrder;
        }
    }
}
