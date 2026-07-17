using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public record Dimensions(decimal WidthCm, decimal HeightCm, decimal DepthCm, decimal WeightKg)
    {
        public static Dimensions Zero() => new(0, 0, 0, 0);

        // Инвариант: размеры техники не могут быть нулевыми или отрицательными при валидации
        public void EnsureValidForShipping()
        {
            if (WidthCm <= 0 || HeightCm <= 0 || DepthCm <= 0 || WeightKg <= 0)
                throw new DomainException("Габариты и вес для доставки электроники должны быть больше нуля.");
        }
    }
}
