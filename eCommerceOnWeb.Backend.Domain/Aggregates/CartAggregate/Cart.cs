//using eCommerceOnWeb.Backend.Domain.Common;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.CartAggregate
//{
//    public class Cart : BaseEntity, IAggregateRoot
//    {
//        public Guid CustomerId { get; private set; }

//        private readonly List<CartItem> _items = new();
//        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

//        private Cart() { }

//        public Cart(Guid customerId)
//        {
//            CustomerId = customerId;
//        }

//        // Бизнес-метод: Добавление товара или увеличение количества
//        public void AddOrUpdateItem(Guid productId, int quantity)
//        {
//            CartItem? existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

//            if (existingItem != null)
//            {
//                // Если товар уже есть — увеличиваем количество
//                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
//            }
//            else
//            {
//                // Если товара нет — создаем новую позицию
//                _items.Add(new CartItem(productId, quantity));
//            }
//        }

//        // Бизнес-метод: Удаление конкретного товара из корзины
//        public void RemoveItem(Guid productId)
//        {
//            CartItem? item = _items.FirstOrDefault(x => x.ProductId == productId);
//            if (item != null)
//            {
//                _items.Remove(item);
//            }
//        }

//        // Бизнес-метод: Очистка корзины (вызывается после успешного оформления заказа)
//        public void Clear()
//        {
//            _items.Clear();
//        }
//    }
//}
