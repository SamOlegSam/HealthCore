using System;
using System.Collections.Generic;

    namespace HealthCore.Models
{
    public class LoginPassword
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int EmployeeID { get; set; }
        public string UserModific { get; set; }
        public DateTime? DateMod { get; set; }
        public List<int> Rol { get; set; }
    }
}
