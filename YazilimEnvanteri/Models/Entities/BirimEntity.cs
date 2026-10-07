using System.ComponentModel.DataAnnotations.Schema;

namespace YazilimEnvanteri.Models.Entities
{
    public class BirimEntity : BaseEntity
    {

        [ForeignKey("YazilimUzmaniEntity")]
        public int YazilimUzmaniId { get; set; }
        public string UstBirim { get; set; } = string.Empty;
        public string AltBirim { get; set; } = string.Empty;
        public string Birim { get; set; } = string.Empty;


    }
}
