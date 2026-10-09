using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Models.Validation;
using YazilimEnvanteri.Services.Implementations;

namespace YazilimEnvanteri.Tests.Integration;

// POST /Proje/UpdateField - the table's inline editing endpoint.
public class ProjeInlineEditTests(PostgresFixture db) : IntegrationTestBase(db)
{
    [Theory]
    [InlineData(AppRoles.SuperAdmin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Admin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Developer, HttpStatusCode.OK)]
    [InlineData(AppRoles.Observer, HttpStatusCode.Forbidden)]
    public async Task Only_project_managers_can_edit_inline(string role, HttpStatusCode expected)
    {
        var (id, _) = await CreateProjeAsync();
        var client = await ClientAsAsync(role);

        var response = await UpdateAsync(client, id, "projeAdi", "Yeni Ad");

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(expected == HttpStatusCode.OK ? "Yeni Ad" : "Envanter", (await StoredAsync(id)).ProjeAdi);
    }

    [Fact]
    public async Task Successful_edit_returns_the_refreshed_row()
    {
        var (id, lookups) = await CreateProjeAsync();
        var otherBirim = await InsertBirimAsync("Diğer Birim");
        var client = await ClientAsAsync(AppRoles.Developer);

        var response = await UpdateAsync(client, id, "birimId", otherBirim.ToString());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal("Diğer Birim", body.GetProperty("project").GetProperty("birim").GetString());
        Assert.Equal(otherBirim, body.GetProperty("project").GetProperty("birimId").GetInt32());
        // Nothing else on the project changed.
        var stored = await StoredAsync(id);
        Assert.Equal(lookups.Uzman, stored.YazilimUzmaniId);
        Assert.Equal("Envanter", stored.ProjeAdi);
    }

    [Theory]
    [InlineData("projeDurum", "6")]
    [InlineData("projeKritiklik", "3")]
    [InlineData("sunucu", "")] // optional, may be cleared
    [InlineData("projeHizmetAlani", "Altyapı")]
    public async Task Allowlisted_fields_save(string field, string value)
    {
        var (id, _) = await CreateProjeAsync();
        var client = await ClientAsAsync(AppRoles.Admin);

        var response = await UpdateAsync(client, id, field, value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("id", "999")]
    [InlineData("silindiMi", "true")]
    [InlineData("SilindiMi", "true")]
    [InlineData("olusturmaTarihi", "2000-01-01")]
    [InlineData("teknolojiId", "1")]
    [InlineData("ProjeAdi", "x")] // names are matched exactly
    public async Task Fields_outside_the_allowlist_are_rejected(string field, string value)
    {
        var (id, _) = await CreateProjeAsync();
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        var response = await UpdateAsync(client, id, field, value);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var stored = await StoredAsync(id);
        Assert.Equal("Envanter", stored.ProjeAdi);
        Assert.NotNull(stored); // still not deleted
    }

    [Theory]
    [InlineData("projeAdi", "", "Proje adı boş bırakılamaz.")]
    [InlineData("projeHizmetAlani", "   ", "Hizmet alanı boş bırakılamaz.")]
    [InlineData("projeDurum", "99", "Geçerli bir durum seçilmelidir.")]
    [InlineData("projeKritiklik", "abc", "Geçerli bir kritiklik seçilmelidir.")]
    [InlineData("birimId", "0", "Birim seçilmelidir.")]
    [InlineData("birimId", "987654", "Seçilen birim bulunamadı.")]
    [InlineData("yazilimUzmaniId", "987654", "Seçilen yazılım uzmanı bulunamadı.")]
    [InlineData("projeKodu", "-23", ProjeKoduKurali.GecersizMesaj)]
    [InlineData("projeKodu", "PRJ-1", ProjeKoduKurali.GecersizMesaj)]
    public async Task Invalid_values_are_rejected_with_a_message(string field, string value, string message)
    {
        var (id, _) = await CreateProjeAsync();
        var client = await ClientAsAsync(AppRoles.Developer);

        var response = await UpdateAsync(client, id, field, value);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal(message, body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Too_long_text_is_rejected()
    {
        var (id, _) = await CreateProjeAsync();
        var client = await ClientAsAsync(AppRoles.Developer);

        var response = await UpdateAsync(client, id, "projeAdi", new string('a', 201));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Project_code_must_stay_unique_and_is_normalized()
    {
        var (first, _) = await CreateProjeAsync();
        var (second, _) = await CreateProjeAsync();
        var firstKod = (await StoredAsync(first)).ProjeKodu;
        var client = await ClientAsAsync(AppRoles.Developer);

        var duplicate = await UpdateAsync(client, second, "projeKodu", firstKod);
        Assert.Contains(ProjeKoduKurali.KullanimdaMesaj, await duplicate.Content.ReadAsStringAsync());

        var newKod = Random.Shared.Next(100_000, 999_999).ToString();
        var ok = await UpdateAsync(client, second, "projeKodu", " " + newKod + " ");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(newKod, (await StoredAsync(second)).ProjeKodu);
    }

    [Fact]
    public async Task Soft_deleted_projects_cannot_be_edited()
    {
        var (id, _) = await CreateProjeAsync();
        await using (var dataSource = Db.CreateDataSourceWithoutForeignKeys())
            await new ProjeService(dataSource).DeleteAsync(id);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        Assert.Equal(HttpStatusCode.NotFound, (await UpdateAsync(client, id, "projeAdi", "Geri Gel")).StatusCode);
    }

    [Fact]
    public async Task Observer_table_is_rendered_read_only()
    {
        await CreateProjeAsync();
        var observer = await (await ClientAsAsync(AppRoles.Observer)).GetStringAsync("/Proje");
        var developer = await (await ClientAsAsync(AppRoles.Developer)).GetStringAsync("/Proje");

        // Cell markup is built client-side from this flag (project-inline-edit.js isEditable).
        Assert.Contains("\"canManageProjects\":false", observer);
        Assert.Contains("\"canManageProjects\":true", developer);
    }

    private static Task<HttpResponseMessage> UpdateAsync(HttpClient client, int id, string field, string value) =>
        client.PostAsJsonAsync("/Proje/UpdateField", new { id, field, value });

    private async Task<ProjeEntity> StoredAsync(int id)
    {
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        await using var c = await dataSource.OpenConnectionAsync();
        return await c.QuerySingleAsync<ProjeEntity>("SELECT * FROM \"Proje\".\"Proje\" WHERE \"Id\" = @id", new { id });
    }

    private async Task<int> InsertBirimAsync(string ad)
    {
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        await using var c = await dataSource.OpenConnectionAsync();
        return await c.ExecuteScalarAsync<int>("""
            INSERT INTO "Birimler" ("Birim", "UstBirim", "AltBirim", "YazilimUzmaniId", "OlusturmaTarihi")
            VALUES (@ad, 'Üst', 'Alt', 0, now()) RETURNING "Id";
            """, new { ad });
    }

    // Real lookup rows (inserted with FK triggers off, see ProjeAuthorizationTests) so the app's
    // FK-enforcing UPDATE accepts them.
    private async Task<(int Id, (int Birim, int Teknoloji, int Uzman) Lookups)> CreateProjeAsync()
    {
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        await using var c = await dataSource.OpenConnectionAsync();
        var birim = await InsertBirimAsync("Test Birimi");
        var teknoloji = await c.ExecuteScalarAsync<int>("""
            INSERT INTO "Teknolojiler" ("BackendTeknoloji", "FrontendTeknoloji", "Veritabani", "EntegrasyonDurum", "eimzaDurum", "OlusturmaTarihi")
            VALUES ('.NET', 'JS', 'PostgreSQL', false, false, now()) RETURNING "Id";
            """);
        var uzman = await c.ExecuteScalarAsync<int>("""
            INSERT INTO "YazilimUzmanlari" ("BirimId", "PersonelId", "ProjeId", "SorumluFirma", "OlusturmaTarihi")
            VALUES (@birim, 0, 0, 'Firma', now()) RETURNING "Id";
            """, new { birim });

        var id = await new ProjeService(dataSource).CreateAsync(new ProjeEntity
        {
            ProjeKodu = Random.Shared.Next(100_000, 999_999).ToString(), ProjeAdi = "Envanter", ProjeHizmetAlani = "Yazılım",
            ProjeAciklamasi = "Açıklama", ProjeAktifMi = true, Sunucu = "srv-01", websiteUrl = "https://example.com",
            BirimId = birim, TeknolojiId = teknoloji, YazilimUzmaniId = uzman,
            ProjeDurum = ProjeDurum.Analiz, ProjeKritiklik = ProjeKritiklik.Orta
        });
        return (id, (birim, teknoloji, uzman));
    }
}
