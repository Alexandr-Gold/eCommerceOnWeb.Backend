using eCommerceOnWeb.Backend.Domain.Entities.Base;

namespace eCommerceOnWeb.Backend.Domain.Entities
{
    public class Product : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }

        // Связь с Категорией (M:1)
        public Guid CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;

        // Связь с Брендом (M:1)
        public Guid BrandId { get; private set; }
        public Brand Brand { get; private set; } = null!;

        protected Product() { }

        public Product(string name, string description, decimal price, int stockQuantity, Guid categoryId, Guid brandId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Имя не пустое.", nameof(name));
            if (price < 0) throw new ArgumentException("Цена >= 0.", nameof(price));
            if (categoryId == Guid.Empty) throw new ArgumentException("Укажите категорию.", nameof(categoryId));
            if (brandId == Guid.Empty) throw new ArgumentException("Укажите бренд.", nameof(brandId));

            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            CategoryId = categoryId;
            BrandId = brandId;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0) throw new ArgumentException("Цена >= 0.");
            Price = newPrice;
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0) throw new ArgumentException("Запас >= 0.");
            StockQuantity = quantity;
        }
    }
}
