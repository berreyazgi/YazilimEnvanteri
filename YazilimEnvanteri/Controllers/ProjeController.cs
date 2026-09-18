using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class ProjeController : Controller
    {
        private readonly IProjeService _projeService;

        public ProjeController(IProjeService projeService)
        {
            _projeService = projeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projeler = await _projeService.GetProjeListAsync();
            return Json(projeler);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var proje = await _projeService.GetProjeDetailAsync(id);
            if (proje is null)
            {
                return NotFound();
            }

            return Json(proje);
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
    }
}
