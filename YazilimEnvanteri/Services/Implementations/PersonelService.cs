using Dapper;
using YazilimEnvanteri.Data.Dapper;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class PersonelService : IPersonelService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PersonelService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<PersonelEntity>> GetAllAsync()
        {
            const string sql = "SELECT * FROM \"Personeller\" ORDER BY \"Id\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<PersonelEntity>(sql);
            return result.AsList();
        }

        public async Task<PersonelEntity?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM \"Personeller\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<PersonelEntity>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(PersonelEntity entity)
        {
            const string sql = """
                INSERT INTO "Personeller" ("BirimId", "Ad", "Soyad", "Email", "Telefon", "Gorev", "SorumluPersonel", "OlusturmaTarihi")
                VALUES (@BirimId, @Ad, @Soyad, @Email, @Telefon, @Gorev, @SorumluPersonel, @OlusturmaTarihi)
                RETURNING "Id";
                """;

            entity.OlusturmaTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(PersonelEntity entity)
        {
            const string sql = """
                UPDATE "Personeller"
                SET "BirimId" = @BirimId,
                    "Ad" = @Ad,
                    "Soyad" = @Soyad,
                    "Email" = @Email,
                    "Telefon" = @Telefon,
                    "Gorev" = @Gorev,
                    "SorumluPersonel" = @SorumluPersonel,
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
            const string sql = "DELETE FROM \"Personeller\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
    }
}
