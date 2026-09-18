using Dapper;
using YazilimEnvanteri.Data.Dapper;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Models.ViewModels;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Services.Implementations
{
    public class ProjeService : IProjeService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProjeService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<ProjeEntity>> GetAllAsync()
        {
            const string sql = "SELECT * FROM \"Proje\".\"Proje\" ORDER BY \"Id\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<ProjeEntity>(sql);
            return result.AsList();
        }

        public async Task<ProjeEntity?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM \"Proje\".\"Proje\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProjeEntity>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(ProjeEntity entity)
        {
            const string sql = """
                INSERT INTO "Proje"."Proje"
                    ("YazilimUzmaniId", "TeknolojiId", "BirimId", "ProjeKodu", "ProjeAdi", "ProjeHizmetAlani",
                     "ProjeAciklamasi", "ProjeAktifMi", "Sunucu", "websiteUrl", "ProjeDurum", "ProjeKritiklik", "OlusturmaTarihi")
                VALUES
                    (@YazilimUzmaniId, @TeknolojiId, @BirimId, @ProjeKodu, @ProjeAdi, @ProjeHizmetAlani,
                     @ProjeAciklamasi, @ProjeAktifMi, @Sunucu, @websiteUrl, @ProjeDurum, @ProjeKritiklik, @OlusturmaTarihi)
                RETURNING "Id";
                """;

            entity.OlusturmaTarihi = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(ProjeEntity entity)
        {
            const string sql = """
                UPDATE "Proje"."Proje"
                SET "YazilimUzmaniId" = @YazilimUzmaniId,
                    "TeknolojiId" = @TeknolojiId,
                    "BirimId" = @BirimId,
                    "ProjeKodu" = @ProjeKodu,
                    "ProjeAdi" = @ProjeAdi,
                    "ProjeHizmetAlani" = @ProjeHizmetAlani,
                    "ProjeAciklamasi" = @ProjeAciklamasi,
                    "ProjeAktifMi" = @ProjeAktifMi,
                    "Sunucu" = @Sunucu,
                    "websiteUrl" = @websiteUrl,
                    "ProjeDurum" = @ProjeDurum,
                    "ProjeKritiklik" = @ProjeKritiklik,
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
            const string sql = "DELETE FROM \"Proje\".\"Proje\" WHERE \"Id\" = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }

        public async Task<IReadOnlyList<ProjeListItemViewModel>> GetProjeListAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.QueryAsync<ProjeListRow>(ProjeListSql);
            return rows.Select(MapToViewModel).ToList();
        }

        public async Task<ProjeListItemViewModel?> GetProjeDetailAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var row = await connection.QuerySingleOrDefaultAsync<ProjeListRow>(
                ProjeListSql + " WHERE p.\"Id\" = @Id;", new { Id = id });
            return row is null ? null : MapToViewModel(row);
        }

        public async Task<DashboardViewModel> GetDashboardSummaryAsync()
        {
            const string sql = "SELECT \"ProjeDurum\", COUNT(*) AS \"Count\" FROM \"Proje\".\"Proje\" GROUP BY \"ProjeDurum\";";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<DurumCountRow>(sql);
            var counts = result.AsList();

            int CountFor(params ProjeDurum[] statuses) =>
                counts.Where(c => statuses.Contains(c.ProjeDurum)).Sum(c => c.Count);

            return new DashboardViewModel
            {
                ToplamProje = counts.Sum(c => c.Count),
                YayindakiProje = CountFor(ProjeDurum.Yayında),
                GelistirmedekiProje = CountFor(ProjeDurum.Geliştirme),
                TestIncelemeProje = CountFor(ProjeDurum.Test, ProjeDurum.İnceleme)
            };
        }

        private sealed class DurumCountRow
        {
            public ProjeDurum ProjeDurum { get; set; }
            public int Count { get; set; }
        }

        // One joined query covering all four related tables, instead of one round trip per
        // table (Proje -> Birim / YazilimUzmani / Teknoloji).
        private const string ProjeListSql = """
            SELECT
                p."Id"                 AS "Id",
                p."ProjeKodu"           AS "ProjeKodu",
                p."ProjeAdi"            AS "ProjeAdi",
                p."ProjeHizmetAlani"    AS "ProjeHizmetAlani",
                p."ProjeAciklamasi"     AS "ProjeAciklamasi",
                p."ProjeAktifMi"        AS "ProjeAktifMi",
                p."Sunucu"              AS "Sunucu",
                p."websiteUrl"          AS "WebsiteUrl",
                p."ProjeDurum"          AS "ProjeDurum",
                p."ProjeKritiklik"      AS "ProjeKritiklik",
                b."Birim"               AS "Birim",
                b."UstBirim"            AS "UstBirim",
                b."AltBirim"            AS "AltBirim",
                y."Ad"                  AS "YazilimUzmaniAd",
                y."Soyad"               AS "YazilimUzmaniSoyad",
                y."KullanıcıAdi"        AS "YazilimUzmaniKullaniciAdi",
                y."Gorev"               AS "YazilimUzmaniGorev",
                y."Email"               AS "YazilimUzmaniEposta",
                y."Telefon"             AS "YazilimUzmaniTelefon",
                y."SorumluFirma"        AS "YazilimUzmaniSorumluFirma",
                t."BackendTeknoloji"    AS "BackendTeknoloji",
                t."FrontendTeknoloji"   AS "FrontendTeknoloji",
                t."Veritabani"          AS "Veritabani",
                t."EntegrasyonDurum"    AS "EntegrasyonDurum",
                t."eimzaDurum"          AS "EimzaDurum"
            FROM "Proje"."Proje" p
            LEFT JOIN "Birimler" b ON b."Id" = p."BirimId"
            LEFT JOIN "YazilimUzmanlari" y ON y."Id" = p."YazilimUzmaniId"
            LEFT JOIN "Teknolojiler" t ON t."Id" = p."TeknolojiId"
            """;

        private static ProjeListItemViewModel MapToViewModel(ProjeListRow r) => new()
        {
            Id = r.Id,
            ProjeKodu = r.ProjeKodu,
            ProjeAdi = r.ProjeAdi,
            ProjeHizmetAlani = r.ProjeHizmetAlani,
            ProjeAciklamasi = r.ProjeAciklamasi,
            ProjeAktifMi = r.ProjeAktifMi,
            Sunucu = r.Sunucu,
            WebsiteUrl = r.WebsiteUrl,
            ProjeDurum = r.ProjeDurum.ToString(),
            ProjeKritiklik = r.ProjeKritiklik.ToString(),

            Birim = r.Birim,
            UstBirim = r.UstBirim,
            AltBirim = r.AltBirim,

            YazilimUzmaniAdSoyad = r.YazilimUzmaniAd is null ? null : $"{r.YazilimUzmaniAd} {r.YazilimUzmaniSoyad}".Trim(),
            YazilimUzmaniKullaniciAdi = r.YazilimUzmaniKullaniciAdi,
            YazilimUzmaniGorev = r.YazilimUzmaniGorev,
            YazilimUzmaniEposta = r.YazilimUzmaniEposta,
            YazilimUzmaniTelefon = r.YazilimUzmaniTelefon?.ToString(),
            YazilimUzmaniSorumluFirma = r.YazilimUzmaniSorumluFirma,

            BackendTeknoloji = r.BackendTeknoloji,
            FrontendTeknoloji = r.FrontendTeknoloji,
            Veritabani = r.Veritabani,
            EntegrasyonDurum = r.EntegrasyonDurum,
            EimzaDurum = r.EimzaDurum
        };

        // Raw shape of the joined query above - kept private since it's an implementation detail
        // of the mapping to ProjeListItemViewModel, not something callers should depend on.
        private sealed class ProjeListRow
        {
            public int Id { get; set; }
            public int ProjeKodu { get; set; }
            public string ProjeAdi { get; set; } = string.Empty;
            public string ProjeHizmetAlani { get; set; } = string.Empty;
            public string ProjeAciklamasi { get; set; } = string.Empty;
            public bool ProjeAktifMi { get; set; }
            public string? Sunucu { get; set; }
            public string? WebsiteUrl { get; set; }
            public ProjeDurum ProjeDurum { get; set; }
            public ProjeKritiklik ProjeKritiklik { get; set; }

            public string? Birim { get; set; }
            public string? UstBirim { get; set; }
            public string? AltBirim { get; set; }

            public string? YazilimUzmaniAd { get; set; }
            public string? YazilimUzmaniSoyad { get; set; }
            public string? YazilimUzmaniKullaniciAdi { get; set; }
            public string? YazilimUzmaniGorev { get; set; }
            public string? YazilimUzmaniEposta { get; set; }
            public int? YazilimUzmaniTelefon { get; set; }
            public string? YazilimUzmaniSorumluFirma { get; set; }

            public string? BackendTeknoloji { get; set; }
            public string? FrontendTeknoloji { get; set; }
            public string? Veritabani { get; set; }
            public bool? EntegrasyonDurum { get; set; }
            public bool? EimzaDurum { get; set; }
        }
    }
}
