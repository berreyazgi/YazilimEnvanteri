using System.Net;
using YazilimEnvanteri.Authorization;

namespace YazilimEnvanteri.Tests.Smoke;

public class HealthTests(PostgresFixture db) : IntegrationTestBase(db)
{
    // app booted + postgres answered SELECT 1; must work without a session (Docker healthcheck)
    [Fact]
    public async Task Health_returns_200_anonymously()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // dashboard query ran against the migrated schema
    [Fact]
    public async Task Dashboard_returns_200_for_signed_in_user()
    {
        var client = await ClientAsAsync(AppRoles.Observer);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
