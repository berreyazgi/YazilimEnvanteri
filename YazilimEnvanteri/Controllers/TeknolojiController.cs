using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class TeknolojiController : Controller
    {
        private readonly ITeknolojiService _teknolojiService;

        public TeknolojiController(ITeknolojiService teknolojiService)
        {
            _teknolojiService = teknolojiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teknolojiler = await _teknolojiService.GetAllAsync();
            return Json(teknolojiler);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var teknoloji = await _teknolojiService.GetByIdAsync(id);
            if (teknoloji is null)
            {
                return NotFound();
            }

            return Json(teknoloji);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeknolojiEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _teknolojiService.CreateAsync(entity);
            return CreatedAtAction(nameof(Details), new { id }, new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] TeknolojiEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _teknolojiService.UpdateAsync(entity);
            return updated ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _teknolojiService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
