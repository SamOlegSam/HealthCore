﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class SummDogZay
    {
        public int Id { get; set; }
        public int ZayavlenieId { get; set; }
        public int? SanatoriumId { get; set; }
        public int DogovorId { get; set; }
        public decimal? Summa { get; set; }
        public string Primech { get; set; }

        public virtual Dogovor Dogovor { get; set; }
        public virtual Zayavlenie Zayavlenie { get; set; }
    }
}