using Dapper;
using YazilimEnvanteri.Data.Dapper;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class YazilimUzmaniService : IYazilimUzmaniService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public YazilimUzmaniService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<YazilimUzmaniEntity>> GetAllAsync()
        {
            const string sql = "SELECT * FROM \"YazilimUzmanlari\" ORDER BY \"Id\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<YazilimUzmaniEntity>(sql);
            return result.AsList();
        }

        public async Task<YazilimUzmaniEntity?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM \"YazilimUzmanlari\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<YazilimUzmaniEntity>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(YazilimUzmaniEntity entity)
        {
            const string sql = """
                INSERT INTO "YazilimUzmanlari" ("ProjeId", "Birim", "KullanıcıAdi", "Ad", "Soyad", "Gorev", "Eposta", "Telefon", "SorumluFirma", "OlusturmaTarihi")
                VALUES (@ProjeId, @Birim, @KullanıcıAdi, @Ad, @Soyad, @Gorev, @Eposta, @Telefon, @SorumluFirma, @OlusturmaTarihi)
                RETURNING "Id";
                """;

            entity.OlusturmaTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(YazilimUzmaniEntity entity)
        {
            const string sql = """
                UPDATE "YazilimUzmanlari"
                SET "ProjeId" = @ProjeId,
                    "Birim" = @Birim,
                    "KullanıcıAdi" = @KullanıcıAdi,
                    "Ad" = @Ad,
                    "Soyad" = @Soyad,
                    "Gorev" = @Gorev,
                    "Eposta" = @Eposta,
                    "Telefon" = @Telefon,
                    "SorumluFirma" = @SorumluFirma,
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
            const string sql = "DELETE FROM \"YazilimUzmanlari\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
    }
}
