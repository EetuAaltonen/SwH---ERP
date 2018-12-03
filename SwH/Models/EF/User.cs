using System;
using System.Collections.Generic;

namespace SwH.Models.EF
{
    public partial class User
    {
        public User()
        {
            Project = new HashSet<Project>();
            ProjectTeam = new HashSet<ProjectTeam>();
            Tasks = new HashSet<Tasks>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }

        public ICollection<Project> Project { get; set; }
        public ICollection<ProjectTeam> ProjectTeam { get; set; }
        public ICollection<Tasks> Tasks { get; set; }
    }
}
