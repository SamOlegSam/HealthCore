﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class Status
    {
        public Status()
        {
            Child = new HashSet<Child>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Priznak { get; set; }
        public string UserMod { get; set; }
        public DateTime? DateMod { get; set; }

        public virtual ICollection<Child> Child { get; set; }
    }
}