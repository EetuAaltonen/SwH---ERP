using System;
using System.Collections.Generic;
using SwH.Models.EF;

namespace SwH.Models
{
    public class ManageProjectsViewModel
    {
        public string  ProjectName{ get; set; }
        public int  ProjectId{ get; set; }
        public int  UserId{ get; set; }
        public int ManagerId { get; set; }
        public DateTime Expires { get; set; }

        public List<User> Managers { get; set; }
    }
}