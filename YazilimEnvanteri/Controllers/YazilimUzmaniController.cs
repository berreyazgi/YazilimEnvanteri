using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Services.Implementations;

namespace YazilimEnvanteri.Controllers
{
    // Personel bilgileriyle (Ad, Soyad, KullanıcıAdi, ...) birleştirilmiş liste - "Yeni Proje"
    // formundaki Yazılım Uzmanı dropdown'ı bunu kullanır (wwwroot/js/modules/project-form.js).
    public class YazilimUzmaniController(YazilimUzmaniService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index() => Json(await service.GetListAsync());
    }
}
