using eCommerceOnWeb.Backend.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.ProductAggregate
{
    // Важно:
    // ProductAttributes сейчас является временным production-friendly MVP решением,
    // а не окончательной моделью каталога.
    //
    // Не расширять его до сложной бизнес-логики.
    //
    // Если появятся:
    // - фильтры по характеристикам;
    // - фасетный поиск;
    // - админка характеристик;
    // - разные наборы характеристик для категорий;
    //
    // тогда создавать отдельную модель Attribute Catalog.

    public record ProductAttributes
    {
        private readonly Dictionary<string, object> _values;

        /// <summary>
        /// Используется System.Text.Json для сериализации.
        ///
        /// Возвращает копию словаря,
        /// чтобы внешний код не мог изменить внутреннее состояние объекта.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, object> Values
            => new(_values);

        /// <summary>
        /// Безопасное чтение значений внутри приложения.
        /// Возвращает только интерфейс чтения.
        /// </summary>
        public IReadOnlyDictionary<string, object> GetValues()
            => _values;

        /// <summary>
        /// Создаёт пустой набор характеристик.
        /// </summary>
        public static ProductAttributes Empty()
            => new(new Dictionary<string, object>());

        /// <summary>
        /// Конструктор используется:
        /// - System.Text.Json при десериализации;
        /// - EF Core при восстановлении объекта.
        ///
        /// Он private намеренно:
        /// создание ProductAttributes должно проходить
        /// через контролируемые методы.
        /// </summary>
        [JsonConstructor]
        private ProductAttributes(Dictionary<string, object>? values)
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

            // После System.Text.Json десериализации
            // значения типа object могут быть JsonElement.
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

        public ProductAttributes SetValue(
            string key,
            object value)
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
                new(_values)
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


            Dictionary<string, object> newValues =
                new(_values);

            newValues.Remove(key);


            return new ProductAttributes(newValues);
        }

        private static bool IsValidType(object value)
            =>
            value is
                string
                or int
                or decimal
                or bool
                or DateTime
                or long
                or double;

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

