using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eCommerceOnWeb.Backend.Persistence.Services
{
    public sealed class StorageService : IStorageService
    {
        private readonly string _baseS3Url;

        public StorageService(IConfiguration configuration)
        {
            // Считываем базовый путь к S3 из конфигурации (если его нет, берем локальный адрес бакета)
            _baseS3Url = configuration["Storage:BaseUrl"]?.TrimEnd('/')
                         ?? "http://localhost:9000/ecommerce-bucket";
        }

        public string GetAbsoluteUrl(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return string.Empty;
            }

            // Если в базе уже лежит готовая ссылка (например, внешнее промо-фото), отдаем её без изменений
            if (storageKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                storageKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return storageKey;
            }

            // Собираем полный путь: http://localhost:9000/ecommerce-bucket/products/имя_файла.jpg
            return $"{_baseS3Url}/{storageKey.TrimStart('/')}";
        }

        public string GenerateStorageKey(string fileName, string folder = "products")
        {
            string extension = Path.GetExtension(fileName);

            // Генерируем уникальное имя на основе Guid, чтобы пользователи случайно не перезаписали файлы друг друга
            return $"{folder}/{Guid.NewGuid()}{extension}".ToLower();
        }
    }
}
