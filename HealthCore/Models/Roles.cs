﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class Roles
    {
        public Roles()
        {
            UserRole = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Role { get; set; }
        public string Priznak { get; set; }

        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}