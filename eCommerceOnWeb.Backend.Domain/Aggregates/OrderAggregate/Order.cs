//using eCommerceOnWeb.Backend.Domain.Common;
//using eCommerceOnWeb.Backend.Domain.Exceptions;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.OrderAggregate
//{
//    public class Order : BaseEntity, IAggregateRoot
//    {
//        public Guid CustomerId { get; private set; }
//        public OrderStatus Status { get; private set; } = OrderStatus.Created;
//        public Money TotalPrice { get; private set; } = Money.Zero();
//        // public DateTimeOffset OrderDate { get; private set; } = DateTimeOffset.Now;
//        //  public Address ShipToAddress { get; private set; }

//        private readonly List<OrderItem> _orderItems = new();
//        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

//        private Order() { } // Используется EF Core при материализации из БД

//        public Order(Guid customerId)
//        {
//            if (customerId == Guid.Empty)
//                throw new DomainException("Идентификатор клиента не может быть пустым.");

//            CustomerId = customerId;
//        }

//        /// <summary>
//        /// Добавление товара в заказ
//        /// </summary>
//        public void AddItem(Guid productId, Money price, int quantity)
//        {
//            if (Status == OrderStatus.Cancelled)
//                throw new DomainException("Нельзя модифицировать отмененный заказ.");

//            if (Status != OrderStatus.Created)
//                throw new DomainException("Изменение состава возможно только для заказов в статусе 'Создан'.");

//            OrderItem? existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
//            if (existingItem != null)
//            {
//                existingItem.AddQuantity(quantity);
//            }
//            else
//            {
//                _orderItems.Add(new OrderItem(productId, price, quantity));
//            }

//            // Пересчитываем общую сумму
//            TotalPrice = new Money(_orderItems.Sum(x => x.Price.Amount * x.Quantity), price.Currency);
//        }

//        /// <summary>
//        /// Перевод заказа в статус "Оплачен"
//        /// </summary>
//        public void Pay()
//        {
//            if (Status == OrderStatus.Cancelled)
//                throw new DomainException("Нельзя оплатить отмененный заказ.");

//            if (Status != OrderStatus.Created)
//                throw new DomainException("Заказ не может быть оплачен в текущем статусе.");

//            Status = OrderStatus.Paid;
//        }

//        /// <summary>
//        /// Бизнес-отмена заказа с фиксацией истории и возвратом остатков
//        /// </summary>
//        public void Cancel()
//        {
//            if (Status == OrderStatus.Cancelled)
//                return; // Идемпотентность: если уже отменен, ничего не делаем

//            if (Status == OrderStatus.Shipped)
//                throw new DomainException("Невозможно отменить заказ, который уже передан в службу доставки.");

//            Status = OrderStatus.Cancelled;

//            // Генерируем доменное событие отмены заказа.
//            // Передаем состав заказа, чтобы обработчик на стороне склада знал, какие товары вернуть в доступные остатки.
//            Dictionary<Guid, int> itemsToReturn = _orderItems.ToDictionary(item => item.ProductId, item => item.Quantity);

//            AddDomainEvent(new OrderCancelledEvent(Id, itemsToReturn, DateTime.UtcNow));
//        }
//    }
//}
