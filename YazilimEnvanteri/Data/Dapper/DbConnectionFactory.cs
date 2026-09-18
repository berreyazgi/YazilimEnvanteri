using System.Data;
using Npgsql;

namespace YazilimEnvanteri.Data.Dapper
{
    // Kept as a standalone factory rather than routing through ApplicationDbContext.GetDbConnection(),
    // even though both ultimately target the same Postgres database via the same "DefaultConnection"
    // connection string:
    //   - ApplicationDbContext is registered only for EF Core migration/schema tooling (see Program.cs)
    //     and is never injected into any controller or service at runtime - there is no live EF Core
    //     connection to share a transaction with.
    //   - Npgsql pools physical connections by connection string at the driver level, so this factory
    //     does not create a second connection pool; it draws from the same pool EF Core would use.
    //   - Every service call opens its own short-lived connection via CreateConnection(), so unrelated
    //     Dapper queries can run concurrently (e.g. Task.WhenAll of independent lookups). Routing
    //     through a scoped DbContext's single connection would serialize all Dapper calls within a
    //     request onto that one connection instead.
    // If EF Core ever starts performing runtime writes/reads alongside Dapper in the same unit of work,
    // revisit this and share the connection via DbContext at that point.
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("'DefaultConnection' connection string is not configured.");
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}
