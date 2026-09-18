using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class BirimController : Controller
    {
        private readonly IBirimService _birimService;

        public BirimController(IBirimService birimService)
        {
            _birimService = birimService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var birimler = await _birimService.GetAllAsync();
            return Json(birimler);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var birim = await _birimService.GetByIdAsync(id);
            if (birim is null)
            {
                return NotFound();
            }

            return Json(birim);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BirimEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _birimService.CreateAsync(entity);
            return CreatedAtAction(nameof(Details), new { id }, new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] BirimEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _birimService.UpdateAsync(entity);
            return updated ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _birimService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
