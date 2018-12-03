using System;
using System.Collections.Generic;
using SwH.Models.EF;

namespace SwH.Models
{
    public class AdminViewModel
    {
        public string  Username{ get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }
}