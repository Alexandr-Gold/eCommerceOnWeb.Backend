using eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate.Exceptions;
using System.Text.Json.Serialization;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    public record ProductAttributes
    {
        private readonly Dictionary<string, object> _values;

        // Добавлено публичное свойство, имя которого СТРОГО совпадает с параметром конструктора (регистр не важен).
        // Атрибут [JsonInclude] заставит System.Text.Json использовать именно его для записи и чтения в БД.
        [JsonInclude]
        public Dictionary<string, object> Values => _values;

        public static ProductAttributes Empty() => new(new Dictionary<string, object>());

        // Теперь параметр 'values' идеально мапится на свойство 'Values' выше!
        [JsonConstructor]
        private ProductAttributes(Dictionary<string, object> values)
        {
            _values = values ?? new Dictionary<string, object>();
        }

        public T? GetValue<T>(string key) =>
            _values.TryGetValue(key, out object? val) && val is T typed ? typed : default;

        public ProductAttributes SetValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException("Ключ атрибута не может быть пустым.");
            if (!IsValidType(value))
                throw new DomainException($"Недопустимый тип значения для атрибута '{key}'.");

            Dictionary<string, object> newValues = new Dictionary<string, object>(_values) { [key] = value };
            return new ProductAttributes(newValues);
        }

        public ProductAttributes RemoveValue(string key)
        {
            if (!_values.ContainsKey(key))
                return this;
            Dictionary<string, object> newValues = new Dictionary<string, object>(_values);
            newValues.Remove(key);
            return new ProductAttributes(newValues);
        }

        public IReadOnlyDictionary<string, object> GetAll() => _values.AsReadOnly();

        private static bool IsValidType(object value) =>
            value is string or int or decimal or bool or DateTime or long or double;

        public static ProductAttributes FromDictionary(Dictionary<string, object>? dict) =>
            dict == null ? Empty() : new ProductAttributes(dict);
    }
}
