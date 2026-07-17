//using eCommerceOnWeb.Backend.Domain.Common;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.OrderAggregate
//{
//    public class OrderItem : BaseEntity
//    {
//        public Guid ProductId { get; private set; }
//        public Money Price { get; private set; }
//        public int Quantity { get; private set; }

//        // Пустой конструктор обязателен для корректной работы EF Core
//        private OrderItem() { }

//        public OrderItem(Guid productId, Money price, int quantity)
//        {
//            if (productId == Guid.Empty)
//                throw new ArgumentException("ProductId не может быть пустым.");

//            if (quantity <= 0)
//                throw new ArgumentException("Количество товара должно быть больше нуля.");

//            Id = Guid.NewGuid(); // Инициализируем уникальный Id сущности
//            ProductId = productId;
//            Price = price ?? throw new ArgumentNullException(nameof(price));
//            Quantity = quantity;
//        }

//        // Бизнес-метод для изменения количества (вызывается из корня агрегата Order)
//        public void AddQuantity(int count)
//        {
//            if (count <= 0)
//                throw new ArgumentException("Добавляемое количество должно быть больше нуля.");

//            Quantity += count;
//        }
//    }
//}
