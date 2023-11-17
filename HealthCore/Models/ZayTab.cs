using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public class ZayTab
    {
        public int Id { get; set; }
        public int? NumberZ { get; set; }
        public DateTime? DateZ { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? S { get; set; }
        public DateTime? Po { get; set; }
        public int? ProtocolId { get; set; }
        public string Who { get; set; }
        public decimal Summa { get; set; }
        public decimal SummaDop { get; set; }
        public int? SanatoriumId { get; set; }
        public string Priznak { get; set; }
        public int TurOpeId { get; set; }
        public int PriznakOplata { get; set; }
        public int Anulirovano { get; set; }
        public string NumberDog { get; set; }
        public DateTime DateDog { get; set; }
        public string UserMod { get; set; }
        public DateTime? DateMod { get; set; }
        public List<int> TableZ { get; set; }
    }
}
