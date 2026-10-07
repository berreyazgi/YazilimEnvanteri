using System.ComponentModel.DataAnnotations.Schema;


namespace YazilimEnvanteri.Models.Entities
{
    public class PersonelEntity: BaseEntity
    {
        //bütün personller için(müdürler ve çevre birimindeki personeller)
        [ForeignKey("SubeEntity")]
        public int BirimId { get; set; }

        //empty olabilir (sisteme giriş hesabı olmayan personeller için)
        public string? KullanıcıAdi { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Gorev { get; set; } = string.Empty;
        
        //empty olabilir
        public string SorumluPersonel{ get; set; } = string.Empty;



    }
}
