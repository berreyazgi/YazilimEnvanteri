
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YazilimEnvanteri.Models.Entities
{
    public class ProjeEntity : BaseEntity
    {
        [ForeignKey("YazilimUzmaniEntity")]
        [Range(1, int.MaxValue, ErrorMessage = "Yazılım uzmanı seçilmelidir.")]
        public int YazilimUzmaniId { get; set; }

        [ForeignKey("TeknolojiEntity")]
        [Range(1, int.MaxValue, ErrorMessage = "Teknoloji seçilmelidir.")]
        public int TeknolojiId { get; set; }

        [ForeignKey("BirimEntity")]
        [Range(1, int.MaxValue, ErrorMessage = "Birim seçilmelidir.")]
        public int BirimId { get; set; }
        public string ProjeKodu { get; set; } = string.Empty;
        public string ProjeAdi { get; set; } = string.Empty;

        //projenin hizmet alanı (yazılım altyapı uygulamaları) gibi
        public string ProjeHizmetAlani { get; set; } = string.Empty;
        public string ProjeAciklamasi { get; set; } = string.Empty;
        public Boolean ProjeAktifMi { get; set; }
        public string Sunucu { get; set; } = string.Empty;
        public string websiteUrl { get; set; } = string.Empty;
        [EnumDataType(typeof(Enums.ProjeDurum), ErrorMessage = "Durum seçilmelidir.")]
        public Enums.ProjeDurum ProjeDurum { get; set; }
        [EnumDataType(typeof(Enums.ProjeKritiklik), ErrorMessage = "Kritiklik seçilmelidir.")]
        public Enums.ProjeKritiklik ProjeKritiklik { get; set; }

        [JsonIgnore]
        public bool SilindiMi { get; set; }


    }
}
