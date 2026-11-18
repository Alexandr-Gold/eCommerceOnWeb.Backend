using Npgsql;
using Respawn;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;
internal static class RespawnCheckpointFactory
{
    public static Task<Respawner> CreateAsync(NpgsqlConnection connection)
    {
        return Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude =
            [
                "public"
            ],
            TablesToIgnore =
            [
                "__EFMigrationsHistory"
            ]
        });
    }
}
