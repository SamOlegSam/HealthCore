﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class Fillial
    {
        public Fillial()
        {
            Employee = new HashSet<Employee>();
            Protocol = new HashSet<Protocol>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Priznak { get; set; }
        public string Prefix { get; set; }
        public string UserMod { get; set; }
        public DateTime? DateMod { get; set; }

        public virtual ICollection<Employee> Employee { get; set; }
        public virtual ICollection<Protocol> Protocol { get; set; }
    }
}