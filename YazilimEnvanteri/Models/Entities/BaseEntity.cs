using System.ComponentModel.DataAnnotations;

namespace YazilimEnvanteri.Models.Entities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? GuncellemeTarihi { get; set; } 
        protected void MarkAsUpdated()
        {
            GuncellemeTarihi = DateTime.UtcNow;
        }
    }
}
