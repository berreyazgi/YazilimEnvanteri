using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.Entities.Enums;
using YazilimEnvanteri.Models.Validation;
using YazilimEnvanteri.Models.ViewModels;
using YazilimEnvanteri.Services.Interfaces;

namespace YazilimEnvanteri.Controllers
{
    // Read actions (list, details, exports) are open to every role; create/edit and soft delete
    // add a stricter policy on top. Hidden buttons in the UI are only a convenience - these
    // attributes are what actually enforce the permission matrix.
    [Authorize(Policy = AuthorizationPolicies.CanViewProjects)]
    public class ProjeController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly IProjeExportService _projeExportService;
        private readonly ILogger<ProjeController> _logger;

        public ProjeController(IProjeService projeService, IProjeExportService projeExportService, ILogger<ProjeController> logger)
        {
            _projeService = projeService;
            _projeExportService = projeExportService;
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
        [Authorize(Policy = AuthorizationPolicies.CanManageProjects)]
        public async Task<IActionResult> Create([FromBody] ProjeEntity entity)
        {
            if (entity is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await ValidateProjeKoduAsync(entity, haricTutulanProjeId: null))
            {
                return BadRequest(ModelState);
            }

            try
            {
                var id = await _projeService.CreateAsync(entity);
                return CreatedAtAction(nameof(Details), new { id }, new { id });
            }
            catch (PostgresException ex) when (TryMapProjeKoduViolation(ex))
            {
                return BadRequest(ModelState);
            }
        }

        // Raw entity (including the BirimId/YazilimUzmaniId/TeknolojiId foreign keys the denormalized
        // ProjeListItemViewModel doesn't carry) so the "Yeni Proje" modal can pre-populate its
        // dropdowns when reused for editing - see wwwroot/js/modules/project-form.js.
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.CanManageProjects)]
        public async Task<IActionResult> GetForEdit(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje is null)
            {
                return NotFound();
            }

            return Json(proje);
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.CanManageProjects)]
        public async Task<IActionResult> Edit(int id, [FromBody] ProjeEntity entity)
        {
            if (entity is null || id != entity.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await ValidateProjeKoduAsync(entity, haricTutulanProjeId: entity.Id))
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _projeService.UpdateAsync(entity);
                return updated ? Ok() : NotFound();
            }
            catch (PostgresException ex) when (TryMapProjeKoduViolation(ex))
            {
                return BadRequest(ModelState);
            }
        }

        // Inline (table cell) edit of a single field - wwwroot/js/modules/project-inline-edit.js.
        // Loads the stored project, changes exactly one allowlisted field (ApplyInlineField), runs that
        // field's rules and saves through the same UpdateAsync as the full Edit form. Responds with the
        // refreshed list row so the table can update locally without reloading.
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.CanManageProjects)]
        public async Task<IActionResult> UpdateField([FromBody] ProjeAlanGuncelleViewModel? request)
        {
            if (request is null) return InlineError("Geçersiz istek.");

            var entity = await _projeService.GetByIdAsync(request.Id);
            if (entity is null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            var error = ApplyInlineField(entity, request.Field, request.Value?.Trim() ?? string.Empty);
            if (error is not null) return InlineError(error);

            // Project code keeps its full rule set (format + uniqueness), as in Create/Edit.
            if (request.Field == "projeKodu" && !await ValidateProjeKoduAsync(entity, haricTutulanProjeId: entity.Id))
                return InlineError(FirstModelError());

            try
            {
                if (!await _projeService.UpdateAsync(entity))
                    return NotFound(new { success = false, message = "Proje bulunamadı." });
            }
            catch (PostgresException ex) when (TryMapProjeKoduViolation(ex))
            {
                return InlineError(FirstModelError());
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return InlineError(request.Field == "birimId" ? "Seçilen birim bulunamadı." : "Seçilen yazılım uzmanı bulunamadı.");
            }

            var proje = await _projeService.GetProjeDetailAsync(entity.Id);
            return Json(new { success = true, project = proje });
        }

        // The allowlist: only these table columns can be changed inline, each with the same limits
        // the full form / database use. Anything else (Id, SilindiMi, timestamps, teknoloji, ...) is
        // rejected - the client-sent field name is only ever compared against these literals.
        private static string? ApplyInlineField(ProjeEntity entity, string? field, string value)
        {
            switch (field)
            {
                case "projeKodu":
                    entity.ProjeKodu = value; // normalized + validated by ValidateProjeKoduAsync
                    return null;
                case "projeAdi":
                    return Text(value, "Proje adı", 200, required: true, v => entity.ProjeAdi = v);
                case "projeHizmetAlani":
                    return Text(value, "Hizmet alanı", 100, required: true, v => entity.ProjeHizmetAlani = v);
                case "sunucu":
                    return Text(value, "Sunucu", 100, required: false, v => entity.Sunucu = v);
                case "projeDurum":
                    if (!int.TryParse(value, out var durum) || !Enum.IsDefined(typeof(ProjeDurum), durum)) return "Geçerli bir durum seçilmelidir.";
                    entity.ProjeDurum = (ProjeDurum)durum;
                    return null;
                case "projeKritiklik":
                    if (!int.TryParse(value, out var kritiklik) || !Enum.IsDefined(typeof(ProjeKritiklik), kritiklik)) return "Geçerli bir kritiklik seçilmelidir.";
                    entity.ProjeKritiklik = (ProjeKritiklik)kritiklik;
                    return null;
                // Existence is enforced by the foreign keys (see the ForeignKeyViolation catch above).
                case "birimId":
                    if (!int.TryParse(value, out var birimId) || birimId < 1) return "Birim seçilmelidir.";
                    entity.BirimId = birimId;
                    return null;
                case "yazilimUzmaniId":
                    if (!int.TryParse(value, out var uzmanId) || uzmanId < 1) return "Yazılım uzmanı seçilmelidir.";
                    entity.YazilimUzmaniId = uzmanId;
                    return null;
                default:
                    return "Bu alan tablo üzerinden düzenlenemez.";
            }

            static string? Text(string value, string label, int maxLength, bool required, Action<string> set)
            {
                if (required && value.Length == 0) return $"{label} boş bırakılamaz.";
                if (value.Length > maxLength) return $"{label} en fazla {maxLength} karakter olabilir.";
                set(value);
                return null;
            }
        }

        private BadRequestObjectResult InlineError(string message) => BadRequest(new { success = false, message });

        private string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault() ?? "Geçersiz değer.";

        // Soft delete (ProjeService.DeleteAsync sets SilindiMi) - SuperAdmin/Admin only.
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.CanDeleteProjects)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _projeService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }

        // Excel/PDF exports for the Projelerim table - the filter parameters mirror the client-side
        // filter state (wwwroot/js/modules/projects.js) so the file reflects what's on screen.
        [HttpGet]
        public async Task<IActionResult> ExportToExcel([FromQuery] ProjeExportFilterViewModel filter)
        {
            var (projeler, _) = await GetProjeListAsync();
            var filtered = ApplyFilter(projeler, filter).ToList();
            var content = _projeExportService.ExportToExcel(filtered);
            var fileName = $"Projelerim_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf([FromQuery] ProjeExportFilterViewModel filter)
        {
            var (projeler, _) = await GetProjeListAsync();
            var filtered = ApplyFilter(projeler, filter).ToList();
            var content = _projeExportService.ExportToPdf(filtered);
            var fileName = $"Projelerim_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(content, "application/pdf", fileName);
        }

        // Same semantics as applyFilters in wwwroot/js/modules/projects.js: values selected inside
        // one filter are OR-ed (Durum = Analiz OR Test), different filters are AND-ed.
        private static IEnumerable<ProjeListItemViewModel> ApplyFilter(
            IEnumerable<ProjeListItemViewModel> projeler, ProjeExportFilterViewModel filter)
        {
            var term = filter.Search?.Trim();

            return projeler.Where(p =>
                (string.IsNullOrEmpty(term) || MatchesSearch(p, term))
                && MatchesAny(filter.HizmetAlani, p.ProjeHizmetAlani)
                && MatchesAny(filter.Birim, p.Birim)
                && MatchesAny(filter.Durum, p.ProjeDurum)
                && MatchesAny(filter.Kritiklik, p.ProjeKritiklik)
                && (!filter.SadeceAktif || p.ProjeAktifMi)
                && (!filter.SadeceWebAdresiOlan || !string.IsNullOrEmpty(p.WebsiteUrl)));
        }

        // An empty selection means "no filter" for that category.
        private static bool MatchesAny(List<string> selected, string? value)
        {
            var values = selected.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            return values.Count == 0
                || (value is not null && values.Any(v => string.Equals(v.Trim(), value, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool MatchesSearch(ProjeListItemViewModel p, string term)
        {
            string?[] haystacks =
            {
                p.ProjeAdi, p.ProjeKodu, p.ProjeHizmetAlani, p.Sunucu,
                p.WebsiteUrl, p.Birim, p.YazilimUzmaniAdSoyad, p.BackendTeknoloji, p.FrontendTeknoloji
            };

            return haystacks.Any(h => !string.IsNullOrEmpty(h) && h.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // Normalizes entity.ProjeKodu in place (" prj-100 " -> "PRJ-100") and records a Turkish
        // ModelState error when it is malformed or already used by another project.
        private async Task<bool> ValidateProjeKoduAsync(ProjeEntity entity, int? haricTutulanProjeId)
        {
            entity.ProjeKodu = ProjeKoduKurali.Normalize(entity.ProjeKodu);

            if (!ProjeKoduKurali.GecerliMi(entity.ProjeKodu))
            {
                ModelState.AddModelError(nameof(ProjeEntity.ProjeKodu), ProjeKoduKurali.GecersizMesaj);
                return false;
            }

            if (await _projeService.ProjeKoduKullaniliyorMuAsync(entity.ProjeKodu, haricTutulanProjeId))
            {
                ModelState.AddModelError(nameof(ProjeEntity.ProjeKodu), ProjeKoduKurali.KullanimdaMesaj);
                return false;
            }

            return true;
        }

        // Covers the race where two saves pass the pre-check at once: the unique index / CHECK
        // constraint still rejects the second one, and the user gets the same friendly message
        // instead of a raw database error.
        private bool TryMapProjeKoduViolation(PostgresException ex)
        {
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation && ex.ConstraintName == "IX_Proje_ProjeKodu")
            {
                ModelState.AddModelError(nameof(ProjeEntity.ProjeKodu), ProjeKoduKurali.KullanimdaMesaj);
                return true;
            }

            if (ex.SqlState == PostgresErrorCodes.CheckViolation && ex.ConstraintName == "CK_Proje_ProjeKodu_Format")
            {
                ModelState.AddModelError(nameof(ProjeEntity.ProjeKodu), ProjeKoduKurali.GecersizMesaj);
                return true;
            }

            return false;
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
