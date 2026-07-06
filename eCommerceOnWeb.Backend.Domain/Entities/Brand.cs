using eCommerceOnWeb.Backend.Domain.Entities.Base;

namespace eCommerceOnWeb.Backend.Domain.Entities
{/// <summary>
 /// Сущность Бренда (Производителя) товара.
 /// </summary>
    public class Brand : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        // Навигационное свойство для связи с товарами данного бренда
        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        protected Brand() { }

        public Brand(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название бренда не может быть пустым.", nameof(name));

            Name = name;
            Description = description;
        }

        public void UpdateDetails(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название бренда не может быть пустым.", nameof(name));

            Name = name;
            Description = description;
        }
    }
}
