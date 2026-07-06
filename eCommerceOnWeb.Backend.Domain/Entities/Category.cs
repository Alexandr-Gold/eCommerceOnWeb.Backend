using eCommerceOnWeb.Backend.Domain.Entities.Base;

namespace eCommerceOnWeb.Backend.Domain.Entities
{
    /// <summary>
    /// Сущность Категории товаров.
    /// Поддерживает иерархическую структуру (дерево категорий).
    /// </summary>
    public class Category : BaseEntity
    {
        // Свойства инкапсулированы (private set) для защиты бизнес-логики
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        // Ссылка на родительскую категорию (для создания подкатегорий, например: Электроника -> Смартфоны)
        public Guid? ParentCategoryId { get; private set; }
        public Category? ParentCategory { get; private set; }

        // Защищенная от прямой модификации коллекция подкатегорий (Инкапсуляция)
        private readonly List<Category> _subCategories = new();
        public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

        // Защищенная от прямой модификации коллекция товаров в этой категории
        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        // Защищенный конструктор без параметров — обязательное требование EF Core для материализации объектов из БД
        protected Category() { }

        /// <summary>
        /// Бизнес-конструктор для безопасного создания новой категории
        /// </summary>
        public Category(string name, string description, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название категории не может быть пустым.", nameof(name));

            Name = name;
            Description = description;
            ParentCategoryId = parentCategoryId;
        }

        /// <summary>
        /// Доменный метод для обновления данных категории (вместо открытых сеттеров)
        /// </summary>
        public void UpdateDetails(string name, string description, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название категории не может быть пустым.", nameof(name));

            // Бизнес-валидация: категория не может быть родительской для самой себя
            if (parentCategoryId == Id && Id != Guid.Empty)
                throw new InvalidOperationException("Категория не может ссылаться на саму себя в качестве родительской.");

            Name = name;
            Description = description;
            ParentCategoryId = parentCategoryId;
        }
    }
}
