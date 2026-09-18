using System.Data;
using Npgsql;

namespace YazilimEnvanteri.Data.Dapper
{
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
