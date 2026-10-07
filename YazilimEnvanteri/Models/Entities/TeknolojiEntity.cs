using System;
using System.Collections.Generic;
using System.Text;

namespace YazilimEnvanteri.Models.Entities
{
    public class TeknolojiEntity : BaseEntity
    {
        public Boolean EntegrasyonDurum { get; set; }
        public string Veritabani { get; set; } = string.Empty;
        public string FrontendTeknoloji { get; set; } = string.Empty;
        public string BackendTeknoloji { get; set; } = string.Empty;
        public Boolean eimzaDurum { get; set; }

      
    }
}
