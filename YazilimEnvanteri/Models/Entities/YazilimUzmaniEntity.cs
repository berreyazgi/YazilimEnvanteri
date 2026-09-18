using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace YazilimEnvanteri.Models.Entities
{
    public class YazilimUzmaniEntity : BaseEntity
    {
        [ForeignKey("ProjeEntity")]
        public int ProjeId { get; set; }
        [ForeignKey("BirimEntity")]
        public int Birim { get; set; }
        public string KullanıcıAdi { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Gorev { get; set; } = string.Empty;
        public string Eposta { get; set; } = string.Empty;
        public int Telefon { get; set; } 
        public string SorumluFirma { get; set; } = string.Empty;

        public YazilimUzmaniEntity() { }

    }
}
