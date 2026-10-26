using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    // Приватный конструктор.
    // Создание Dimensions разрешено только через фабричный метод Create(),
    // чтобы гарантировать выполнение доменных инвариантов.
    // Любой экземпляр Dimensions должен быть валидным.
    public record Dimensions
    {
        public decimal WidthCm { get; init; }
        public decimal HeightCm { get; init; }
        public decimal DepthCm { get; init; }
        public decimal WeightKg { get; init; }

        private Dimensions(
            decimal widthCm,
            decimal heightCm,
            decimal depthCm,
            decimal weightKg)
        {
            WidthCm = widthCm;
            HeightCm = heightCm;
            DepthCm = depthCm;
            WeightKg = weightKg;
        }


        public static Dimensions Create(
            decimal widthCm,
            decimal heightCm,
            decimal depthCm,
            decimal weightKg)
        {
            if (widthCm <= 0 ||
                heightCm <= 0 ||
                depthCm <= 0 ||
                weightKg <= 0)
            {
                throw new DomainException(
                    "Габариты и вес для доставки электроники должны быть больше нуля.");
            }

            return new Dimensions(
                widthCm,
                heightCm,
                depthCm,
                weightKg);
        }
    }
}

