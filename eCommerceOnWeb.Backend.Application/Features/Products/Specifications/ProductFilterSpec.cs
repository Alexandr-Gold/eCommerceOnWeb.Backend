using Ardalis.Specification;
using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate;

namespace eCommerceOnWeb.Backend.Application.Features.Products.Specifications
{
    public sealed class ProductFilterSpec : Specification<Product>
    {
        public ProductFilterSpec(
            Guid? categoryId,
            Guid? brandId,
            string? searchTerm,
            int pageIndex = 1,
            int pageSize = 10,
            bool isPagingEnabled = true) // ДОБАВЛЕН Параметр по умолчанию
        {
            // 1. Фильтрация по Категории и Бренду
            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                Query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue && brandId != Guid.Empty)
            {
                Query.Where(p => p.BrandId == brandId.Value);
            }

            // 2. Текстовый поиск по имени или SKU
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string search = searchTerm.Trim().ToLower();
                Query.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
            }

            // Применяем сортировку и пагинацию, только если флаг включен
            if (isPagingEnabled)
            {
                // 3. Упорядочивание
                Query.OrderBy(p => p.Name);

                // 4. Постраничный вывод
                Query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize);
            }
        }
    }
}
