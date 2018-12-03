using System;
using System.Collections.Generic;
using SwH.Models.EF;

namespace SwH.Models
{
    public class ManageTasksViewModel
    {
        public string  TaskName{ get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string  Descr{ get; set; }
        public string  Status{ get; set; }
        public DateTime Expires { get; set; }

        public List<User> Employees { get; set; }
        public List<Project> Projects { get; set; }
    }
}