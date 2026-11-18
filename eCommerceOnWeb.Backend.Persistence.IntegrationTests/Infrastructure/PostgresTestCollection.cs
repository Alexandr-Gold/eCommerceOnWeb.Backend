namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;

[CollectionDefinition(
    Name,
    DisableParallelization = true)]
public sealed class PostgresTestCollection
    : ICollectionFixture<PostgresTestDatabaseFixture>
{
    internal const string Name = "PostgreSQL";
}