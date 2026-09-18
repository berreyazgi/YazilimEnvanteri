using Dapper;
using YazilimEnvanteri.Data.Dapper;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class BirimService : IBirimService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BirimService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<BirimEntity>> GetAllAsync()
        {
            const string sql = "SELECT * FROM \"Birimler\" ORDER BY \"Id\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<BirimEntity>(sql);
            return result.AsList();
        }

        public async Task<BirimEntity?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM \"Birimler\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<BirimEntity>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(BirimEntity entity)
        {
            const string sql = """
                INSERT INTO "Birimler" ("YazilimUzmaniId", "UstBirim", "AltBirim", "Birim", "OlusturmaTarihi")
                VALUES (@YazilimUzmaniId, @UstBirim, @AltBirim, @Birim, @OlusturmaTarihi)
                RETURNING "Id";
                """;

            entity.OlusturmaTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(BirimEntity entity)
        {
            const string sql = """
                UPDATE "Birimler"
                SET "YazilimUzmaniId" = @YazilimUzmaniId,
                    "UstBirim" = @UstBirim,
                    "AltBirim" = @AltBirim,
                    "Birim" = @Birim,
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
            const string sql = "DELETE FROM \"Birimler\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
    }
}
