using eCommerceOnWeb.Backend.Application.Common.Interfaces;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;
internal sealed class TestStorageService : IStorageService
{
    public string GetAbsoluteUrl(string storageKey)
        => $"https://test.local/{storageKey}";

    public string GenerateStorageKey(string fileName, string folder = "products")
        => $"{folder}/{Guid.NewGuid():N}-{fileName}";
}
