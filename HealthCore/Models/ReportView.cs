using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public class ReportView
    {
        public int Id { get; set; }
        public DateTime? DateS { get; set; }
        public DateTime? DatePO { get; set; }
        public List<int> Rol { get; set; }
        public string UserModific { get; set; }
        public DateTime? DateMod { get; set; }
    }
}
