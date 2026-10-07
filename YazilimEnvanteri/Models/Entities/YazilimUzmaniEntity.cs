using System.ComponentModel.DataAnnotations.Schema;

namespace YazilimEnvanteri.Models.Entities
{
    public class YazilimUzmaniEntity : BaseEntity
    {
        // Kimlik/iletişim bilgileri (KullanıcıAdi, Ad, Soyad, Email, Gorev, Telefon) PersonelEntity'de
        // tutulur; burada yalnızca personele referans ve yazılım uzmanına özgü alanlar kalır.
        [ForeignKey("PersonelEntity")]
        public int PersonelId { get; set; }
        [ForeignKey("ProjeEntity")]
        public int ProjeId { get; set; }
        [ForeignKey("BirimEntity")]
        public int BirimId { get; set; }
        public string SorumluFirma { get; set; } = string.Empty;

        public YazilimUzmaniEntity() { }

    }
}
