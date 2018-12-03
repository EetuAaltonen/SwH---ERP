using System;
using System.Collections.Generic;
using SwH.Models.EF;

namespace SwH.Models
{
    public class ManageProjectTeamsViewModel
    {
        public int ProjectId{ get; set; }
        public List<User> Employees{ get; set; }
        public List<User> ProjectTeam{ get; set; }
    }
}