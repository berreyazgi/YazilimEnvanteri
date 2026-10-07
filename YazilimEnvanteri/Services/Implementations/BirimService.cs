using Dapper;
using Npgsql;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Implementations
{
    public class BirimService(NpgsqlDataSource dataSource)
    {
        public async Task<IReadOnlyList<BirimEntity>> GetAllAsync()
        {
            using var connection = dataSource.CreateConnection();
            return (await connection.QueryAsync<BirimEntity>("SELECT * FROM \"Birimler\" ORDER BY \"Id\";")).AsList();
        }
    }
}
