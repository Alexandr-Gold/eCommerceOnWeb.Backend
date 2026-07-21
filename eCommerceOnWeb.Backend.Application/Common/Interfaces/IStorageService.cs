namespace eCommerceOnWeb.Backend.Application.Common.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Превращает ключ хранения (StorageKey) в полный абсолютный URL для фронтенда.
        /// </summary>
        /// <param name="storageKey">Уникальный ключ файла в бакете (например, "products/iphone15-main.jpg")</param>
        /// <returns>Полная ссылка (например, "https://my-eshop.com")</returns>
        string GetAbsoluteUrl(string storageKey);

        /// <summary>
        /// Генерирует уникальный ключ хранения для нового файла перед его загрузкой.
        /// </summary>
        string GenerateStorageKey(string fileName, string folder = "products");
    }
}
