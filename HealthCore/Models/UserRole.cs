﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class UserRole
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        public string Priznak { get; set; }
        public string UserModif { get; set; }
        public DateTime? DateModif { get; set; }

        public virtual Roles Role { get; set; }
        public virtual User User { get; set; }
    }
}