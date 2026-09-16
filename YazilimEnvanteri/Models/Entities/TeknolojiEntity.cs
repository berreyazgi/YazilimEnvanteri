using System;
using System.Collections.Generic;
using System.Text;

namespace YazilimEnvanteri.Models.Entities
{
    public class TeknolojiEntity : BaseEntity
    {
        public Boolean EntegrasyonDurum { get; set; }
        public string Veritabani { get; set; }
        public string FrontendTeknoloji { get; set; }
        public string BackendTeknoloji { get; set; }
        public Boolean eimzaDurum { get; set; }

        public TeknolojiEntity() { }

    }
}
