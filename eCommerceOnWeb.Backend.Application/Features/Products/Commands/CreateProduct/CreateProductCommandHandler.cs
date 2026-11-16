using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private const string SkuUniqueIndexName = "IX_products_sku";
    private const string GtinUniqueIndexName = "IX_products_gtin";
    private const string CategoryFkConstraintName = "FK_products_categories_category_id";
    private const string BrandFkConstraintName = "FK_products_brands_brand_id";

    private readonly IECommerceDbContext _context;
    private readonly IDbConstraintViolationDetector _constraintDetector;   // NEW

    public CreateProductCommandHandler(
        IECommerceDbContext context,
        IDbConstraintViolationDetector constraintDetector)
    {
        _context = context;
        _constraintDetector = constraintDetector;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Нормализация SKU для pre-check уникальности.
        string normalizedSku = request.Sku
            .Trim()
            .ToUpperInvariant();

        // Pre-check — оптимизация UX, НЕ защита от гонок.
        // Гарантия уникальности — unique index в БД (см. catch DbUpdateException ниже).
        //
        // IgnoreQueryFilters() включает soft-deleted товары: их SKU остаётся
        // зарезервированным (исторические заказы/ERP ссылаются на него).
        // NB: если в проекте появится tenant-filter — этот вызов обойдёт и его,
        // сделав SKU уникальным МЕЖДУ тенантами. При необходимости — заменить
        // на IgnoreQueryFilters(selective) + Where(TenantId == ...).
        bool skuExists = await _context.Products
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Sku == normalizedSku, cancellationToken);

        if (skuExists)
        {
            return Result<Guid>.Failure(
                ProductErrors.SkuAlreadyExists(normalizedSku));
        }

        string? normalizedGtin = string.IsNullOrWhiteSpace(request.Gtin)
            ? null
            : request.Gtin.Trim();

        if (normalizedGtin is not null)
        {
            bool gtinExists = await _context.Products
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Gtin == normalizedGtin, cancellationToken);

            if (gtinExists)
            {
                return Result<Guid>.Failure(
                    ProductErrors.GtinAlreadyExists(normalizedGtin));
            }
        }

        Product product;

        try
        {
            Money price = Money.Create(
                request.PriceAmount,
                request.PriceCurrency);

            Dimensions dimensions = Dimensions.Create(
                request.DimWidthCm,
                request.DimHeightCm,
                request.DimDepthCm,
                request.DimWeightKg);

            product = Product.Create(
                request.Name,
                normalizedSku,
                normalizedGtin,
                request.ModelNumber,
                price,
                request.CategoryId,
                request.BrandId,
                request.Description,
                request.WarrantyMonths,
                dimensions,
                request.InitialStock);

            // FIX: null-safe. request.Attributes == null — валидатор должен
            // отклонять, но handler не должен падать с NRE при прямом вызове
            // (внутренние сценарии: миграция, seed, bulk-import).
            if (request.Attributes is { Count: > 0 })
            {
                // NB: временно используем SetAttribute в цикле.
                // После добавления Product.SetInitialAttributes(dict) — заменить,
                // чтобы избежать O(N²) аллокаций Dictionary и N лишних
                // ProductAttributeChangedEvent на одно создание.
                foreach (ProductAttributeDto attribute in request.Attributes)
                {
                    object value = ConvertAttributeValue(attribute.Value);
                    product.SetAttribute(attribute.Key, value);
                }
            }
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(
                ProductErrors.InvalidCreateProductRequest(ex.Message));
        }

        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex) when (
            ex.GetBaseException() is not OperationCanceledException)  // FIX: регрессия
        {
            return MapDbUpdateException(
                ex,
                normalizedSku,
                normalizedGtin,
                request.CategoryId,
                request.BrandId);
        }

        return Result<Guid>.Success(product.Id);
    }

    private Result<Guid> MapDbUpdateException(
        DbUpdateException ex,
        string normalizedSku,
        string? normalizedGtin,
        Guid categoryId,
        Guid brandId)
    {
        // FIX: Npgsql-специфика вынесена в IDbConstraintViolationDetector
        // (реализация — в Persistence). Application не знает про Postgres.
        if (!_constraintDetector.TryDetect(ex, out ConstraintViolation violation))
        {
            return Result<Guid>.Failure(ProductErrors.PersistenceError());
        }

        switch (violation.Kind)
        {
            case ConstraintKind.Unique:
                return violation.ConstraintName switch
                {
                    SkuUniqueIndexName =>
                        Result<Guid>.Failure(
                            ProductErrors.SkuAlreadyExists(normalizedSku)),

                    GtinUniqueIndexName when normalizedGtin is not null =>
                        Result<Guid>.Failure(
                            ProductErrors.GtinAlreadyExists(normalizedGtin)),

                    _ => Result<Guid>.Failure(ProductErrors.PersistenceError())
                };

            // NEW: FK violations — клиент видит «категория/бренд не найдены»,
            // а не generic «ошибка хранилища».
            case ConstraintKind.ForeignKey:
                return violation.ConstraintName switch
                {
                    CategoryFkConstraintName =>
                        Result<Guid>.Failure(
                            ProductErrors.CategoryNotFound(categoryId)),

                    BrandFkConstraintName =>
                        Result<Guid>.Failure(
                            ProductErrors.BrandNotFound(brandId)),

                    _ => Result<Guid>.Failure(ProductErrors.PersistenceError())
                };

            default:
                return Result<Guid>.Failure(ProductErrors.PersistenceError());
        }
    }

    private static object ConvertAttributeValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            // FIX: GetString() контрактно nullable — без `!`
            JsonValueKind.String =>
                value.GetString()
                ?? throw new DomainException(
                    "Строковое значение атрибута не может быть null."),

            JsonValueKind.Number when value.TryGetInt64(out long number) =>
                number,

            JsonValueKind.Number when value.TryGetDecimal(out decimal number) =>
                number,

            JsonValueKind.True => true,

            JsonValueKind.False => false,

            JsonValueKind.Null =>
                throw new DomainException(
                    "Атрибут не может иметь значение null."),

            JsonValueKind.Number =>
                throw new DomainException(
                    "Числовое значение атрибута выходит за допустимый диапазон."),

            _ =>
                throw new DomainException(
                    "Неподдерживаемый тип значения атрибута.")
        };
    }
}

