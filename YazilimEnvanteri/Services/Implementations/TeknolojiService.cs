using Dapper;
using Npgsql;
using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Implementations
{
    public class TeknolojiService(NpgsqlDataSource dataSource)
    {
        public async Task<IReadOnlyList<TeknolojiEntity>> GetAllAsync()
        {
            using var connection = dataSource.CreateConnection();
            return (await connection.QueryAsync<TeknolojiEntity>("SELECT * FROM \"Teknolojiler\" ORDER BY \"Id\";")).AsList();
        }
    }
}
