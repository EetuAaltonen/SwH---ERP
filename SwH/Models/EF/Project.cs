using System;
using System.Collections.Generic;

namespace SwH.Models.EF
{
    public partial class Project
    {
        public Project()
        {
            ProjectTeam = new HashSet<ProjectTeam>();
            Tasks = new HashSet<Tasks>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Manager { get; set; }
        public DateTime Expires { get; set; }

        public User ManagerNavigation { get; set; }
        public ICollection<ProjectTeam> ProjectTeam { get; set; }
        public ICollection<Tasks> Tasks { get; set; }
    }
}
