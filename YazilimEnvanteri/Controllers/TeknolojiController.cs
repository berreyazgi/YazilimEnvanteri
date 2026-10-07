using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Services.Implementations;

namespace YazilimEnvanteri.Controllers
{
    // Read-only lookup for the "Yeni Proje" form dropdown (wwwroot/js/modules/project-form.js).
    public class TeknolojiController(TeknolojiService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index() => Json(await service.GetAllAsync());
    }
}
