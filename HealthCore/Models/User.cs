﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HealthCore.Models
{
    public partial class User
    {
        public User()
        {
            UserRole = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int? EmployeeId { get; set; }
        public string UserModif { get; set; }
        public DateTime? DateMod { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}