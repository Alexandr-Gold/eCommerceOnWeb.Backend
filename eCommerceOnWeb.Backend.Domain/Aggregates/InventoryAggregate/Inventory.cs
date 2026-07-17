//using eCommerceOnWeb.Backend.Domain.Common;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.InventoryAggregate
//{
//    public class Inventory : BaseEntity, IAggregateRoot
//    {
//        public Guid ProductId { get; private set; }
//        public int AvailableQuantity { get; private set; } // Доступно для покупки
//        public int ReservedQuantity { get; private set; }  // Зарезервировано в созданных заказах

//        // Общее количество на складе = Доступно + Резерв
//        public int TotalQuantity => AvailableQuantity + ReservedQuantity;

//        private Inventory() { } // Для EF Core

//        public Inventory(Guid productId, int initialQuantity)
//        {
//            if (initialQuantity < 0) throw new ArgumentException("Начальное количество не может быть отрицательным.");
//            ProductId = productId;
//            AvailableQuantity = initialQuantity;
//            ReservedQuantity = 0;
//        }

//        // Бизнес-метод: Пополнение склада новыми поставками
//        public void Restock(int quantity)
//        {
//            if (quantity <= 0) throw new ArgumentException("Количество для пополнения должно быть больше нуля.");
//            AvailableQuantity += quantity;
//        }

//        // Бизнес-метод: Резервирование под новый заказ
//        public void Reserve(int quantity)
//        {
//            if (quantity <= 0) throw new ArgumentException("Количество для резерва должно быть больше нуля.");
//            if (quantity > AvailableQuantity)
//                throw new InvalidOperationException($"Недостаточно товара на складе. Запрошено: {quantity}, Доступно: {AvailableQuantity}");

//            AvailableQuantity -= quantity;
//            ReservedQuantity += quantity;
//        }

//        // Бизнес-метод: Подтверждение покупки (товар физически уезжает со склада, резерв списывается)
//        public void ConfirmSale(int quantity)
//        {
//            if (quantity <= 0) throw new ArgumentException("Количество должно быть больше нуля.");
//            if (quantity > ReservedQuantity)
//                throw new InvalidOperationException("Попытка списать больше товара, чем было зарезервировано.");

//            ReservedQuantity -= quantity;
//        }

//        // Бизнес-метод: Отмена заказа (возвращаем резерв обратно в доступные остатки)
//        public void CancelReservation(int quantity)
//        {
//            if (quantity <= 0) throw new ArgumentException("Количество должно быть больше нуля.");
//            if (quantity > ReservedQuantity)
//                throw new InvalidOperationException("Попытка отменить резерв на большее количество, чем зарезервировано.");

//            ReservedQuantity -= quantity;
//            AvailableQuantity += quantity;
//        }
//    }
//}
