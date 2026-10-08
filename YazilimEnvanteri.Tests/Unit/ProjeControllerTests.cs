using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using YazilimEnvanteri.Controllers;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Models.Validation;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Tests.Unit;

public class ProjeControllerTests
{
    private readonly Mock<IProjeService> _service = new();

    private ProjeController CreateController() =>
        new(_service.Object, Mock.Of<IProjeExportService>(), NullLogger<ProjeController>.Instance);

    private static ProjeEntity Proje(string kod) => new()
    {
        ProjeKodu = kod, ProjeAdi = "Envanter", YazilimUzmaniId = 1, TeknolojiId = 1, BirimId = 1,
        ProjeDurum = ProjeDurum.Analiz, ProjeKritiklik = ProjeKritiklik.Orta
    };

    [Fact]
    public async Task Create_normalizes_code_and_saves()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<ProjeEntity>())).ReturnsAsync(42);

        var result = await CreateController().Create(Proje(" 100 "));

        Assert.IsType<CreatedAtActionResult>(result);
        _service.Verify(s => s.ProjeKoduKullaniliyorMuAsync("100", null));
        _service.Verify(s => s.CreateAsync(It.Is<ProjeEntity>(p => p.ProjeKodu == "100")));
    }

    [Fact]
    public async Task Create_rejects_code_already_in_use_without_saving()
    {
        _service.Setup(s => s.ProjeKoduKullaniliyorMuAsync("100", null)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Create(Proje("100"));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ProjeKoduKurali.KullanimdaMesaj, controller.ModelState[nameof(ProjeEntity.ProjeKodu)]!.Errors.Single().ErrorMessage);
        _service.Verify(s => s.CreateAsync(It.IsAny<ProjeEntity>()), Times.Never);
    }
}
