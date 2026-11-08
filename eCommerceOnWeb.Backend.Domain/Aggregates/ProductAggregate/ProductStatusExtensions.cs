namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;

/*
* ProductStatusExtensions содержит простые правила работы со статусом.
*
* Текущая модель использует enum, так как количество состояний
* и переходов ограничено.
*
* При усложнении lifecycle продукта:
* - увеличение количества состояний;
* - сложные правила переходов;
* - разные разрешённые действия для ролей;
*
* перейти на Smart Enum.
*/

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
