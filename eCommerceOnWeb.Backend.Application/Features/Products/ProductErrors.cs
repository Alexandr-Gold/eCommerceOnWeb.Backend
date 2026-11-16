using eCommerceOnWeb.Backend.Domain.Common;

namespace eCommerceOnWeb.Backend.Application.Features.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
       new(
           "Products.NotFound",
           $"Товар с идентификатором '{id}' не найден.");

    public static Error SkuAlreadyExists(string sku) =>
        new(
            "Products.SkuAlreadyExists",
            $"Товар с SKU '{sku}' уже существует.");

    public static Error GtinAlreadyExists(string gtin) =>
        new(
            "Products.GtinAlreadyExists",
            $"Товар с GTIN '{gtin}' уже существует.");

    // NEW: FK violations — клиент видит «категория не найдена»,
    // а не generic «ошибка хранилища».
    public static Error CategoryNotFound(Guid id) =>
        new(
            "Products.CategoryNotFound",
            $"Категория с идентификатором '{id}' не найдена.");

    // NEW: то же для бренда.
    public static Error BrandNotFound(Guid id) =>
        new(
            "Products.BrandNotFound",
            $"Бренд с идентификатором '{id}' не найден.");

    public static Error InvalidCreateProductRequest(string message) =>
        new(
            "Products.InvalidCreateProductRequest",
            message);

    public static Error PersistenceError() =>
        new(
            "Products.PersistenceError",
            "Не удалось сохранить товар из-за ошибки хранилища. " +
            "Повторите попытку позже.");     // FIX: добавлено «повторите» —
                                             // клиент не звонит в поддержку
                                             // при транзиентной ошибке
}
