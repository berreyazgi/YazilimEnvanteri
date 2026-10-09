using System.Net;
using System.Net.Http.Json;
using Dapper;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Services.Implementations;

namespace YazilimEnvanteri.Tests.Integration;

// The permission matrix, enforced on the server: every request here is sent by hand (no UI), with a
// valid antiforgery token, so a 403 means the role check rejected it even though the button that
// would normally send it is hidden.
public class ProjeAuthorizationTests(PostgresFixture db) : IntegrationTestBase(db)
{
    public static TheoryData<string> AllRoles => [AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Developer, AppRoles.Observer];

    [Theory]
    [InlineData(AppRoles.SuperAdmin, HttpStatusCode.Created)]
    [InlineData(AppRoles.Admin, HttpStatusCode.Created)]
    [InlineData(AppRoles.Developer, HttpStatusCode.Created)]
    [InlineData(AppRoles.Observer, HttpStatusCode.Forbidden)]
    public async Task Create(string role, HttpStatusCode expected)
    {
        var lookups = await SeedLookupsAsync();
        var client = await ClientAsAsync(role);

        var response = await client.PostAsJsonAsync("/Proje/Create", Payload(0, NewKod(), lookups));

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.SuperAdmin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Admin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Developer, HttpStatusCode.OK)]
    [InlineData(AppRoles.Observer, HttpStatusCode.Forbidden)]
    public async Task Edit(string role, HttpStatusCode expected)
    {
        var lookups = await SeedLookupsAsync();
        var kod = NewKod();
        var id = await CreateProjeAsync(kod, lookups);
        var client = await ClientAsAsync(role);

        var response = await client.PostAsJsonAsync($"/Proje/Edit/{id}", Payload(id, kod, lookups, ad: "Yeni Ad"));

        Assert.Equal(expected, response.StatusCode);
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        var stored = await new ProjeService(dataSource).GetByIdAsync(id);
        Assert.Equal(expected == HttpStatusCode.OK ? "Yeni Ad" : "Envanter", stored!.ProjeAdi);
    }

    [Theory]
    [MemberData(nameof(AllRoles))]
    public async Task Edit_form_data_is_only_served_to_project_managers(string role)
    {
        var id = await CreateProjeAsync(NewKod(), await SeedLookupsAsync());
        var client = await ClientAsAsync(role);

        var response = await client.GetAsync($"/Proje/GetForEdit/{id}");

        Assert.Equal(role == AppRoles.Observer ? HttpStatusCode.Forbidden : HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.SuperAdmin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Admin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Developer, HttpStatusCode.Forbidden)]
    [InlineData(AppRoles.Observer, HttpStatusCode.Forbidden)]
    public async Task Delete_is_a_soft_delete_for_superadmin_and_admin_only(string role, HttpStatusCode expected)
    {
        var id = await CreateProjeAsync(NewKod(), await SeedLookupsAsync());
        var client = await ClientAsAsync(role);

        var response = await client.PostAsync($"/Proje/Delete/{id}", null);

        Assert.Equal(expected, response.StatusCode);

        // Row is never physically removed; only SilindiMi changes.
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        await using var connection = await dataSource.OpenConnectionAsync();
        var silindiMi = await connection.QuerySingleAsync<bool>("SELECT \"SilindiMi\" FROM \"Proje\".\"Proje\" WHERE \"Id\" = @id", new { id });
        Assert.Equal(expected == HttpStatusCode.OK, silindiMi);

        var details = await client.GetAsync($"/Proje/Details/{id}");
        Assert.Equal(expected == HttpStatusCode.OK ? HttpStatusCode.NotFound : HttpStatusCode.OK, details.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllRoles))]
    public async Task Every_role_can_view_filter_and_export_projects(string role)
    {
        var id = await CreateProjeAsync(NewKod(), await SeedLookupsAsync());
        var client = await ClientAsAsync(role);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Proje")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/Proje/Details/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Proje/ExportToExcel?Search=Envanter&SadeceAktif=true")).StatusCode);
    }

    [Fact]
    public async Task Anonymous_requests_are_rejected()
    {
        var lookups = await SeedLookupsAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, (await Client.GetAsync("/Proje")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Client.PostAsJsonAsync("/Proje/Create", Payload(0, NewKod(), lookups))).StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllRoles))]
    public async Task Ui_only_renders_actions_the_role_may_use(string role)
    {
        var id = await CreateProjeAsync(NewKod(), await SeedLookupsAsync());
        var client = await ClientAsAsync(role);
        var canManage = role != AppRoles.Observer;
        var canDelete = role is AppRoles.SuperAdmin or AppRoles.Admin;

        var list = await client.GetStringAsync("/Proje");
        Assert.Equal(canManage, list.Contains("data-open-create-proje"));
        Assert.Equal(canManage, list.Contains("data-create-proje-overlay"));
        Assert.Contains("data-project-filter", list); // filters for everyone
        Assert.Contains($"\"canManageProjects\":{canManage.ToString().ToLowerInvariant()}", list);
        Assert.Contains($"\"canDeleteProjects\":{canDelete.ToString().ToLowerInvariant()}", list);
        Assert.Contains($"\"canManageUsers\":{(role == AppRoles.SuperAdmin).ToString().ToLowerInvariant()}", list);

        var details = await client.GetStringAsync($"/Proje/Details/{id}");
        Assert.Equal(canManage, details.Contains("data-edit-proje"));
        Assert.Equal(canDelete, details.Contains("data-delete-proje"));
    }

    private static string NewKod() => Random.Shared.Next(100_000, 999_999).ToString(); // tables are shared across tests

    private static object Payload(int id, string kod, (int Birim, int Teknoloji, int Uzman) l, string ad = "Envanter") => new
    {
        id, projeKodu = kod, projeAdi = ad, projeHizmetAlani = "Yazılım", projeAciklamasi = "Açıklama", projeAktifMi = true,
        sunucu = "srv-01", websiteUrl = "https://example.com",
        birimId = l.Birim, teknolojiId = l.Teknoloji, yazilimUzmaniId = l.Uzman,
        projeDurum = (int)ProjeDurum.Analiz, projeKritiklik = (int)ProjeKritiklik.Orta
    };

    // Real lookup rows so the app's (FK-enforcing) Create succeeds; inserted with FK triggers off
    // because Birim <-> YazilimUzmani reference each other.
    private async Task<(int Birim, int Teknoloji, int Uzman)> SeedLookupsAsync()
    {
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        await using var c = await dataSource.OpenConnectionAsync();
        var birim = await c.ExecuteScalarAsync<int>("""
            INSERT INTO "Birimler" ("Birim", "UstBirim", "AltBirim", "YazilimUzmaniId", "OlusturmaTarihi")
            VALUES ('Test Birimi', 'Üst', 'Alt', 0, now()) RETURNING "Id";
            """);
        var teknoloji = await c.ExecuteScalarAsync<int>("""
            INSERT INTO "Teknolojiler" ("BackendTeknoloji", "FrontendTeknoloji", "Veritabani", "EntegrasyonDurum", "eimzaDurum", "OlusturmaTarihi")
            VALUES ('.NET', 'JS', 'PostgreSQL', false, false, now()) RETURNING "Id";
            """);
        var uzman = await c.ExecuteScalarAsync<int>("""
            INSERT INTO "YazilimUzmanlari" ("BirimId", "PersonelId", "ProjeId", "SorumluFirma", "OlusturmaTarihi")
            VALUES (@birim, 0, 0, 'Firma', now()) RETURNING "Id";
            """, new { birim });
        return (birim, teknoloji, uzman);
    }

    private async Task<int> CreateProjeAsync(string kod, (int Birim, int Teknoloji, int Uzman) l)
    {
        await using var dataSource = Db.CreateDataSourceWithoutForeignKeys();
        return await new ProjeService(dataSource).CreateAsync(new ProjeEntity
        {
            ProjeKodu = kod, ProjeAdi = "Envanter", ProjeHizmetAlani = "Yazılım", ProjeAciklamasi = "Açıklama", ProjeAktifMi = true,
            Sunucu = "srv-01", websiteUrl = "https://example.com",
            BirimId = l.Birim, TeknolojiId = l.Teknoloji, YazilimUzmaniId = l.Uzman,
            ProjeDurum = ProjeDurum.Analiz, ProjeKritiklik = ProjeKritiklik.Orta
        });
    }
}
