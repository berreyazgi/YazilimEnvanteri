using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.ViewModels;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class ProjeController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly ILogger<ProjeController> _logger;

        public ProjeController(IProjeService projeService, ILogger<ProjeController> logger)
        {
            _projeService = projeService;
            _logger = logger;
        }

        // Projelerim page - the project list/search/filter/table/pagination UI that used to live
        // on the Home page.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (projeler, dataError) = await GetProjeListAsync();
            ViewBag.DataError = dataError;
            return View(projeler);
        }

        // Human-facing project detail page (no JS consumer currently calls this route for JSON,
        // so it serves the Razor view directly rather than duplicating it under a different action).
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var (proje, dataError) = await GetProjeDetailAsync(id);
            ViewBag.DataError = dataError;

            if (proje == null && !dataError)
            {
                return NotFound();
            }

            return View(proje);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjeEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _projeService.CreateAsync(entity);
            return CreatedAtAction(nameof(Details), new { id }, new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] ProjeEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _projeService.UpdateAsync(entity);
            return updated ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _projeService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
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
                _logger.LogError(ex, "Proje listesi alınırken hata oluştu.");
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
                _logger.LogError(ex, "Proje detayı alınırken hata oluştu.");
                return (null, true);
            }
        }
    }
}
