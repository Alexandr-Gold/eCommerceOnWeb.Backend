namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public static class ProductStatusExtensions
    {
        public static bool IsTerminal(
            this ProductStatus status)
        {
            return status == ProductStatus.Discontinued;
        }


        public static bool IsSellable(
            this ProductStatus status)
        {
            return status == ProductStatus.Active;
        }


        public static bool IsEditable(
            this ProductStatus status)
        {
            return status != ProductStatus.Discontinued;
        }
    }
}
