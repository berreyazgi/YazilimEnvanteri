using Dapper;
using YazilimEnvanteri.Data.Dapper;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class TeknolojiService : ITeknolojiService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TeknolojiService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<TeknolojiEntity>> GetAllAsync()
        {
            const string sql = "SELECT * FROM \"Teknolojiler\" ORDER BY \"Id\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<TeknolojiEntity>(sql);
            return result.AsList();
        }

        public async Task<TeknolojiEntity?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM \"Teknolojiler\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<TeknolojiEntity>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(TeknolojiEntity entity)
        {
            const string sql = """
                INSERT INTO "Teknolojiler" ("EntegrasyonDurum", "Veritabani", "FrontendTeknoloji", "BackendTeknoloji", "eimzaDurum", "OlusturmaTarihi")
                VALUES (@EntegrasyonDurum, @Veritabani, @FrontendTeknoloji, @BackendTeknoloji, @eimzaDurum, @OlusturmaTarihi)
                RETURNING "Id";
                """;

            entity.OlusturmaTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(TeknolojiEntity entity)
        {
            const string sql = """
                UPDATE "Teknolojiler"
                SET "EntegrasyonDurum" = @EntegrasyonDurum,
                    "Veritabani" = @Veritabani,
                    "FrontendTeknoloji" = @FrontendTeknoloji,
                    "BackendTeknoloji" = @BackendTeknoloji,
                    "eimzaDurum" = @eimzaDurum,
                    "GuncellemeTarihi" = @GuncellemeTarihi
                WHERE "Id" = @Id;
                """;

            entity.GuncellemeTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, entity);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM \"Teknolojiler\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
    }
}
