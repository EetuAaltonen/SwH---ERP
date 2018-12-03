using System;
using System.Collections.Generic;

namespace SwH.Models.EF
{
    public partial class Tasks
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string Descr { get; set; }
        public string Status { get; set; }
        public DateTime Expires { get; set; }

        public Project Project { get; set; }
        public User User { get; set; }
    }
}
