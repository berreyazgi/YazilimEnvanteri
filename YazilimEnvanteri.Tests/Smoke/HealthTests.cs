using System.Net;

namespace YazilimEnvanteri.Tests.Smoke;

public class HealthTests(PostgresFixture db) : IntegrationTestBase(db)
{
    [Theory]
    [InlineData("/health")] // app booted + postgres answered SELECT 1
    [InlineData("/")]       // dashboard query ran against the migrated schema
    public async Task Endpoint_returns_200(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
