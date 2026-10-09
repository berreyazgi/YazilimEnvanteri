using System.Net;
using System.Net.Http.Json;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Validation;

namespace YazilimEnvanteri.Tests.Integration;

public class ProjeEndpointTests(PostgresFixture db) : IntegrationTestBase(db)
{
    [Fact]
    public async Task Create_with_non_numeric_code_returns_400_with_turkish_message()
    {
        var client = await ClientAsAsync(AppRoles.Developer);

        var response = await client.PostAsJsonAsync("/Proje/Create", new
        {
            projeKodu = "PRJ-ABC", projeAdi = "Envanter", yazilimUzmaniId = 1, teknolojiId = 1, birimId = 1,
            projeDurum = 1, projeKritiklik = 2
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(ProjeKoduKurali.GecersizMesaj, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Birim_lookup_returns_json_array_from_database()
    {
        var client = await ClientAsAsync(AppRoles.Observer);

        var birimler = await client.GetFromJsonAsync<List<object>>("/Birim");

        Assert.NotNull(birimler);
    }
}
