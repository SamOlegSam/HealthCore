﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class Protocol
    {
        public Protocol()
        {
            Zayavlenie = new HashSet<Zayavlenie>();
        }

        public int Id { get; set; }
        public int? NumberP { get; set; }
        public DateTime? DateProt { get; set; }
        public string Priznak { get; set; }
        public string UserModific { get; set; }
        public DateTime? DateModific { get; set; }
        public int? FilialId { get; set; }

        public virtual Fillial Filial { get; set; }
        public virtual ICollection<Zayavlenie> Zayavlenie { get; set; }
    }
}