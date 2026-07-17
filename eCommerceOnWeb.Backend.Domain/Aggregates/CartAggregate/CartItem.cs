//using eCommerceOnWeb.Backend.Domain.Common;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.CartAggregate
//{
//    public class CartItem : BaseEntity
//    {
//        public Guid ProductId { get; private set; }
//        public int Quantity { get; private set; }

//        private CartItem() { }

//        public CartItem(Guid productId, int quantity)
//        {
//            UpdateQuantity(quantity);
//            ProductId = productId;
//        }

//        public void UpdateQuantity(int quantity)
//        {
//            if (quantity <= 0) throw new ArgumentException("Количество товара в корзине должно быть больше нуля.");
//            Quantity = quantity;
//        }
//    }
//}
