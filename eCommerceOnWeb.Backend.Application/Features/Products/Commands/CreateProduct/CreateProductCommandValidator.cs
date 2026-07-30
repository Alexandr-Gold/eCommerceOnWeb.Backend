using FluentValidation;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Queries.CreateProduct
{
    /// <summary>
    /// Валидатор входящих данных для команды создания продукта.
    /// Проверяет базовые технические контракты до того, как данные попадут в Домен.
    /// </summary>
    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя товара обязательно для заполнения.")
                .MaximumLength(250).WithMessage("Имя товара не должно превышать 250 символов.");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("Внутренний артикул (SKU) обязателен.")
                .MaximumLength(100).WithMessage("SKU не должен превышать 100 символов.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание товара обязательно.")
                .MaximumLength(2000).WithMessage("Описание не должно превышать 2000 символов.");

            // Проверяем международный штрих-код EAN-13, если он передан
            RuleFor(x => x.Gtin)
                .MaximumLength(13).WithMessage("Штрих-код (GTIN) не может быть длиннее 13 символов.")
                .Matches(@"^\d{13}$").When(x => !string.IsNullOrEmpty(x.Gtin))
                .WithMessage("Штрих-код должен состоять строго из 13 цифр (формат EAN-13).");

            RuleFor(x => x.ModelNumber)
                .NotEmpty().WithMessage("Партномер (Model Number) обязателен для электроники.")
                .MaximumLength(100).WithMessage("Номер модели не должен превышать 100 символов.");

            RuleFor(x => x.PriceAmount)
                .GreaterThan(0).WithMessage("Цена товара должна быть больше нуля.");

            RuleFor(x => x.PriceCurrency)
                .NotEmpty().WithMessage("Укажите валюту.")
                .Length(3).WithMessage("Код валюты должен состоять строго из 3 символов ISO (например, RUB).");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Идентификатор категории обязателен.");

            RuleFor(x => x.BrandId)
                .NotEmpty().WithMessage("Идентификатор бренда обязателен.");

            RuleFor(x => x.WarrantyMonths)
                .GreaterThanOrEqualTo(0).WithMessage("Срок гарантии не может быть отрицательным.");

            // Валидация габаритов электроники (согласно доменным инвариантам Dimensions)
            RuleFor(x => x.DimWidthCm)
                .GreaterThan(0).WithMessage("Ширина упаковки должна быть больше нуля.");

            RuleFor(x => x.DimHeightCm)
                .GreaterThan(0).WithMessage("Высота упаковки должна быть больше нуля.");

            RuleFor(x => x.DimDepthCm)
                .GreaterThan(0).WithMessage("Глубина упаковки должна быть больше нуля.");

            RuleFor(x => x.DimWeightKg)
                .GreaterThan(0).WithMessage("Вес товара должен быть больше нуля.");

            RuleFor(x => x.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("Начальный остаток на складе не может быть отрицательным.");
        }
    }
}
