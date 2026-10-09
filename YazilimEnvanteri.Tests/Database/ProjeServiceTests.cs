using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Services.Implementations;

namespace YazilimEnvanteri.Tests.Database;

[Collection(PostgresCollection.Name)]
public class ProjeServiceTests(PostgresFixture db)
{
    [Fact]
    public async Task Soft_delete_hides_project_and_frees_its_code()
    {
        await using var dataSource = db.CreateDataSourceWithoutForeignKeys();
        var service = new ProjeService(dataSource);
        var kod = Random.Shared.Next(100_000, 999_999).ToString(); // tables are shared across tests

        var id = await service.CreateAsync(new ProjeEntity
        {
            ProjeKodu = kod, ProjeAdi = "Envanter", YazilimUzmaniId = 1, TeknolojiId = 1, BirimId = 1,
            ProjeDurum = ProjeDurum.Analiz, ProjeKritiklik = ProjeKritiklik.Orta
        });

        Assert.True(await service.ProjeKoduKullaniliyorMuAsync(kod));
        Assert.False(await service.ProjeKoduKullaniliyorMuAsync(kod, haricTutulanProjeId: id));
        Assert.Equal("Analiz", (await service.GetProjeDetailAsync(id))!.ProjeDurum);

        Assert.True(await service.DeleteAsync(id));

        Assert.Null(await service.GetByIdAsync(id));
        Assert.Null(await service.GetProjeDetailAsync(id));
        Assert.DoesNotContain(await service.GetProjeListAsync(), p => p.Id == id);
        Assert.False(await service.ProjeKoduKullaniliyorMuAsync(kod));
        Assert.False(await service.DeleteAsync(id));
    }
}
