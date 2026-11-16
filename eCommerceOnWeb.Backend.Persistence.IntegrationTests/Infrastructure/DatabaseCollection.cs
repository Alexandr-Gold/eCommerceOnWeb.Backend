namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

[CollectionDefinition(
    "Database",
    DisableParallelization = true)]
public sealed class DatabaseCollection
    : ICollectionFixture<DatabaseFixture>
{
}
