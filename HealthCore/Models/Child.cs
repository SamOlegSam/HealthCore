﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class Child
    {
        public Child()
        {
            TableZay = new HashSet<TableZay>();
        }

        public int Id { get; set; }
        public string Fio { get; set; }
        public int? EmployeeId { get; set; }
        public int? StatusId { get; set; }
        public string Pol { get; set; }
        public DateTime? DateBirth { get; set; }
        public string UserMod { get; set; }
        public DateTime? DateMod { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<TableZay> TableZay { get; set; }
    }
}