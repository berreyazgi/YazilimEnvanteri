using System.Net;
using System.Net.Http.Json;
using YazilimEnvanteri.Models.Validation;

namespace YazilimEnvanteri.Tests.Integration;

public class ProjeEndpointTests(PostgresFixture db) : IntegrationTestBase(db)
{
    [Fact]
    public async Task Create_with_non_numeric_code_returns_400_with_turkish_message()
    {
        var response = await Client.PostAsJsonAsync("/Proje/Create", new
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
        var birimler = await Client.GetFromJsonAsync<List<object>>("/Birim");

        Assert.NotNull(birimler);
    }
}
