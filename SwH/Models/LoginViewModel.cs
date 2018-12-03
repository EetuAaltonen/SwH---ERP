using System;
using System.Collections.Generic;
using SwH.Models.EF;

namespace SwH.Models
{
    public class LoginViewModel
    {
        public string OldPassword{ get; set; }
        public string NewPassword { get; set; }
        public string ConfPassword { get; set; }
    }
}