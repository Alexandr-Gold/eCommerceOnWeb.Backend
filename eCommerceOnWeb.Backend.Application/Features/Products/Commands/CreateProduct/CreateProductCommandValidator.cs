using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;
using FluentValidation;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Название товара обязательно.")
            .MaximumLength(ProductConstraints.NameMaxLength)
            .WithMessage(
                $"Название товара не должно превышать " +
                $"{ProductConstraints.NameMaxLength} символов.");

        RuleFor(x => x.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("SKU товара обязателен.")
            .MaximumLength(ProductConstraints.SkuMaxLength)
            .WithMessage(
                $"SKU не должен превышать " +
                $"{ProductConstraints.SkuMaxLength} символов.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Описание товара обязательно.")
            .MaximumLength(ProductConstraints.DescriptionMaxLength)
            .WithMessage(
                $"Описание не должно превышать " +
                $"{ProductConstraints.DescriptionMaxLength} символов.");

        RuleFor(x => x.Gtin)
            .Must(BeValidGtin)
            .WithMessage(
                "GTIN должен состоять ровно из 13 цифр (формат EAN-13).");

        RuleFor(x => x.ModelNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Партномер обязателен.")
            .MaximumLength(ProductConstraints.ModelNumberMaxLength)
            .WithMessage(
                $"Партномер не должен превышать " +
                $"{ProductConstraints.ModelNumberMaxLength} символов.");

        RuleFor(x => x.PriceAmount)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена не может быть отрицательной.")
            .Must(HaveValidScale)
            .WithMessage(
                "Цена может содержать не более двух знаков после запятой.");

        RuleFor(x => x.PriceCurrency)
            .Must(Money.IsSupportedCurrency)
            .WithMessage(
                "Поддерживаются валюты USD, EUR и RUB.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Категория обязательна.");

        RuleFor(x => x.BrandId)
            .NotEmpty()
            .WithMessage("Бренд обязателен.");

        RuleFor(x => x.WarrantyMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage(
                "Срок гарантии не может быть отрицательным.");

        RuleFor(x => x.DimWidthCm)
            .GreaterThan(0)
            .WithMessage(
                "Ширина упаковки должна быть больше нуля.");

        RuleFor(x => x.DimHeightCm)
            .GreaterThan(0)
            .WithMessage(
                "Высота упаковки должна быть больше нуля.");

        RuleFor(x => x.DimDepthCm)
            .GreaterThan(0)
            .WithMessage(
                "Глубина упаковки должна быть больше нуля.");

        RuleFor(x => x.DimWeightKg)
            .GreaterThan(0)
            .WithMessage(
                "Вес товара должен быть больше нуля.");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage(
                "Начальный остаток не может быть отрицательным.");

        RuleFor(x => x.Attributes)
            .NotNull()
            .WithMessage(
                "Коллекция атрибутов не может быть null.");

        RuleFor(x => x.Attributes)
            .Must(HaveUniqueAttributeKeys)
            .WithMessage(
                "Ключи атрибутов не должны повторяться.");

        RuleForEach(x => x.Attributes)
            .ChildRules(attribute =>
            {
                attribute.RuleFor(x => x.Key)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(
                        "Ключ атрибута обязателен.")
                    .MaximumLength(
                        ProductConstraints.AttributeKeyMaxLength)
                    .WithMessage(
                        $"Ключ атрибута не должен превышать " +
                        $"{ProductConstraints.AttributeKeyMaxLength} символов.");

                attribute.RuleFor(x => x.Value)
                    .Must(IsSupportedAttributeValue)
                    .WithMessage(
                        "Значение атрибута должно быть строкой, числом или boolean.");
            });
    }


    private static bool BeValidGtin(string? gtin)
    {
        if (string.IsNullOrWhiteSpace(gtin))
            return true;

        string value = gtin.Trim();

        return value.Length == ProductConstraints.GtinMaxLength
               && value.All(char.IsDigit);
    }


    private static bool HaveValidScale(decimal value)
    {
        int scale = (decimal.GetBits(value)[3] >> 16) & 0xFF;

        return scale <= 2;
    }


    private static bool HaveUniqueAttributeKeys(
        IReadOnlyCollection<ProductAttributeDto>? attributes)
    {
        if (attributes is null)
            return true;

        return attributes
            .Select(x => x.Key.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count()
            == attributes.Count;
    }


    private static bool IsSupportedAttributeValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => true,

            JsonValueKind.Number =>
                value.TryGetInt64(out _)
                || value.TryGetDecimal(out _),

            JsonValueKind.True => true,

            JsonValueKind.False => true,

            _ => false
        };
    }
}