using Dapper;
using Npgsql;
using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Services.Implementations
{
    public class YazilimUzmaniService(NpgsqlDataSource dataSource)
    {
        // Joined with Personeller/Birimler so callers get the specialist's name, e-mail etc.
        // PersonelId is NOT NULL with an FK, so the Personeller join is always satisfied.
        public async Task<IReadOnlyList<YazilimUzmaniListItemViewModel>> GetListAsync()
        {
            const string sql = """
                SELECT
                    y."Id"              AS "Id",
                    y."PersonelId"      AS "PersonelId",
                    y."ProjeId"         AS "ProjeId",
                    y."BirimId"         AS "BirimId",
                    b."Birim"           AS "Birim",
                    y."SorumluFirma"    AS "SorumluFirma",
                    per."KullanıcıAdi"  AS "KullaniciAdi",
                    per."Ad"            AS "Ad",
                    per."Soyad"         AS "Soyad",
                    per."Email"         AS "Email",
                    per."Gorev"         AS "Gorev",
                    per."Telefon"       AS "Telefon"
                FROM "YazilimUzmanlari" y
                INNER JOIN "Personeller" per ON per."Id" = y."PersonelId"
                LEFT JOIN "Birimler" b ON b."Id" = y."BirimId"
                ORDER BY per."Ad", per."Soyad";
                """;

            using var connection = dataSource.CreateConnection();
            return (await connection.QueryAsync<YazilimUzmaniListItemViewModel>(sql)).AsList();
        }
    }
}
