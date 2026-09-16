using System.ComponentModel.DataAnnotations.Schema;

namespace YazilimEnvanteri.Models.Entities
{
    public class BirimEntity : BaseEntity
    {

        [ForeignKey("YazilimUzmaniEntity")]
        public int YazilimUzmaniId { get; set; }
        public string UstBirim { get; set; } 
        public string AltBirim { get; set; } 
        public string Birim { get; set; } 
        public BirimEntity() { }

    }
}
