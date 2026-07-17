//namespace eCommerceOnWeb.Backend.Domain.Aggregates.OrderAggregate
//{
//    public record Money
//    {
//        public decimal Amount { get; init; }
//        public string Currency { get; init; }

//        // Конструктор по умолчанию необходим для десериализации EF Core / Redis
//        private Money()
//        {
//            Currency = "USD";
//        }

//        public Money(decimal amount, string currency)
//        {
//            if (amount < 0)
//                throw new ArgumentException("Сумма не может быть отрицательной.");

//            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
//                throw new ArgumentException("Код валюты должен состоять из 3-х символов ISO.");

//            Amount = amount;
//            Currency = currency.ToUpper();
//        }

//        // Фабричный метод для создания нулевого баланса (удобно для инициализации заказа)
//        public static Money Zero(string currency = "USD") => new(0, currency);
//    }
//}
