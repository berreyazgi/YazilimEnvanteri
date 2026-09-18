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
            try
            {
                var dashboard = await _projeService.GetDashboardSummaryAsync();
                ViewBag.DataError = false;
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gösterge paneli verileri alınırken hata oluştu.");
                ViewBag.DataError = true;
                return View(new DashboardViewModel());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
