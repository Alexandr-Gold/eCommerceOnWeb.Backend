namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Exceptions
{
    // Наследуется от глобального, но лежит внутри агрегата товара
    public class ProductSpecificationException : DomainException
    {
        public ProductSpecificationException(string message) : base(message) { }
    }
}
