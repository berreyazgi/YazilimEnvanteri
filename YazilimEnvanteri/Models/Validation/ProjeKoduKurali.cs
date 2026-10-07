using System.Globalization;
using System.Text.RegularExpressions;

namespace YazilimEnvanteri.Models.Validation
{
    // Single source of truth for what a Proje Kodu may look like. The same pattern is enforced by a
    // CHECK constraint on "Proje"."Proje" (see ProjeConfiguration) and mirrored client-side in
    // wwwroot/js/modules/project-form.js.
    public static partial class ProjeKoduKurali
    {
        public const int MaxLength = 50;

        // DB CHECK constraint only (ProjeConfiguration) - looser than the app rule below so older
        // alphanumeric codes already in the table stay valid.
        public const string Desen = "^[A-Z0-9ÇĞİÖŞÜ][A-Z0-9ÇĞİÖŞÜ_-]*$";

        // App rule for new/edited codes: digits only.
        public const string SayisalDesen = "^[0-9]+$";

        public const string GecersizMesaj = "Proje kodu yalnızca rakamlardan oluşmalıdır (ör. 100, 9999).";

        public const string KullanimdaMesaj = "Bu proje kodu başka bir projede kullanılıyor.";

        private static readonly CultureInfo Turkce = CultureInfo.GetCultureInfo("tr-TR");

        [GeneratedRegex(SayisalDesen)]
        private static partial Regex DesenRegex();

        // " prj-100 " -> "PRJ-100". Turkish casing so "i" becomes "İ", matching how users type.
        public static string Normalize(string? kod) => (kod ?? string.Empty).Trim().ToUpper(Turkce);

        public static bool GecerliMi(string normalizedKod) =>
            normalizedKod.Length is > 0 and <= MaxLength && DesenRegex().IsMatch(normalizedKod);
    }
}
