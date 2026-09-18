using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YazilimEnvanteri.Models;
using YazilimEnvanteri.Models.ViewModels;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProjeService projeService, ILogger<HomeController> logger)
        {
            _projeService = projeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var (projeler, dataError) = await GetProjeListAsync();
            ViewBag.DataError = dataError;
            return View(projeler);
        }

        public async Task<IActionResult> ProjeDetay(int id)
        {
            var (proje, dataError) = await GetProjeDetailAsync(id);
            ViewBag.DataError = dataError;

            if (proje == null && !dataError)
            {
                return NotFound();
            }

            return View(proje);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<(List<ProjeListItemViewModel> Projeler, bool DataError)> GetProjeListAsync()
        {
            try
            {
                var projeler = await _projeService.GetProjeListAsync();
                return (projeler.ToList(), false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje verileri alınırken hata oluştu.");
                return (new List<ProjeListItemViewModel>(), true);
            }
        }

        private async Task<(ProjeListItemViewModel? Proje, bool DataError)> GetProjeDetailAsync(int id)
        {
            try
            {
                var proje = await _projeService.GetProjeDetailAsync(id);
                return (proje, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje verileri alınırken hata oluştu.");
                return (null, true);
            }
        }
    }
}
