using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    public class PersonelController : Controller
    {
        private readonly IPersonelService _personelService;

        public PersonelController(IPersonelService personelService)
        {
            _personelService = personelService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var personeller = await _personelService.GetAllAsync();
            return Json(personeller);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var personel = await _personelService.GetByIdAsync(id);
            if (personel is null)
            {
                return NotFound();
            }

            return Json(personel);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PersonelEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _personelService.CreateAsync(entity);
            return CreatedAtAction(nameof(Details), new { id }, new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] PersonelEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _personelService.UpdateAsync(entity);
            return updated ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _personelService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
