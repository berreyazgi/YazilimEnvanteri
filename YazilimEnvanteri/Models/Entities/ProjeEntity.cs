
using System.ComponentModel.DataAnnotations.Schema;

namespace YazilimEnvanteri.Models.Entities
{
    public class ProjeEntity : BaseEntity
    {
        [ForeignKey("YazilimUzmaniEntity")]
        public int YazilimUzmaniId { get; set; }

        [ForeignKey("TeknolojiEntity")]
        public int TeknolojiId { get; set; }

        [ForeignKey("BirimEntity")]
        public int BirimId { get; set; }
        public int ProjeKodu { get; set; }
        public string ProjeAdi { get; set; } = string.Empty;

        //projenin hizmet alanı (yazılım altyapı uygulamaları) gibi
        public string ProjeHizmetAlani { get; set; } = string.Empty;
        public string ProjeAciklamasi { get; set; } = string.Empty;
        public Boolean ProjeAktifMi { get; set; }
        public string Sunucu { get; set; }
        public string websiteUrl { get; set; }
        public Enums.ProjeDurum ProjeDurum { get; set; }
        public Enums.ProjeKritiklik ProjeKritiklik { get; set; }

        public ProjeEntity() { }


    }
}
