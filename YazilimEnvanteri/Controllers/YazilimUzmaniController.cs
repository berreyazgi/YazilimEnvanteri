using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class YazilimUzmaniController : Controller
    {
        private readonly IYazilimUzmaniService _yazilimUzmaniService;

        public YazilimUzmaniController(IYazilimUzmaniService yazilimUzmaniService)
        {
            _yazilimUzmaniService = yazilimUzmaniService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var uzmanlar = await _yazilimUzmaniService.GetAllAsync();
            return Json(uzmanlar);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var uzman = await _yazilimUzmaniService.GetByIdAsync(id);
            if (uzman is null)
            {
                return NotFound();
            }

            return Json(uzman);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YazilimUzmaniEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _yazilimUzmaniService.CreateAsync(entity);
            return CreatedAtAction(nameof(Details), new { id }, new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] YazilimUzmaniEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _yazilimUzmaniService.UpdateAsync(entity);
            return updated ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _yazilimUzmaniService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
