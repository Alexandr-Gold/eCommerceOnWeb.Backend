using eCommerceOnWeb.Backend.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    //    Важно:
    //ProductAttributes сейчас является временным production-friendly MVP решением,
    //а не окончательной моделью каталога.

    //Не расширять его до сложной бизнес-логики.
    //Если появятся:
    //- фильтры по характеристикам;
    //- фасетный поиск;
    //- админка характеристик;
    //- разные наборы характеристик для категорий;

    //тогда создавать отдельную модель Attribute Catalog.

    public record ProductAttributes
    {
        private readonly Dictionary<string, object> _values;

        public IReadOnlyDictionary<string, object> GetValues()
            => _values;

        public static ProductAttributes Empty() => new(new Dictionary<string, object>());

        // Конструктор используется System.Text.Json при десериализации.
        // Имя параметра совпадает с внутренним состоянием объекта.
        [JsonConstructor]
        private ProductAttributes(Dictionary<string, object> values)
        {
            _values = values == null
                 ? new Dictionary<string, object>()
                 : new Dictionary<string, object>(values);
        }

        public T? GetValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException(
                    "Ключ атрибута не может быть пустым.");

            if (!_values.TryGetValue(key, out object? value)
                || value is null)
            {
                return default;
            }

            if (value is JsonElement element)
            {
                try
                {
                    return element.Deserialize<T>();
                }
                catch (JsonException)
                {
                    return default;
                }
            }

            return value is T typed
                ? typed
                : default;
        }

        public ProductAttributes SetValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException(
                    "Ключ атрибута не может быть пустым.");

            if (value is null)
                throw new DomainException(
                    $"Значение атрибута '{key}' не может быть null.");

            if (!IsValidType(value))
                throw new DomainException(
                    $"Недопустимый тип значения для атрибута '{key}'.");

            Dictionary<string, object> newValues =
                new Dictionary<string, object>(_values)
                {
                    [key] = value
                };

            return new ProductAttributes(newValues);
        }

        public ProductAttributes RemoveValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException(
                    "Ключ атрибута не может быть пустым.");

            if (!_values.ContainsKey(key))
                return this;

            Dictionary<string, object> newValues = new(_values);
            newValues.Remove(key);

            return new ProductAttributes(newValues);
        }

        private static bool IsValidType(object value) =>
            value is string or int or decimal or bool or DateTime or long or double;

        public static ProductAttributes FromDictionary(
    Dictionary<string, object>? dict)
        {
            if (dict == null)
                return Empty();

            return new ProductAttributes(
                new Dictionary<string, object>(dict));
        }
    }
}
