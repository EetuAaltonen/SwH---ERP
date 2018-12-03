using System;
using System.Collections.Generic;

namespace SwH.Models.EF
{
    public partial class ProjectTeam
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        public Project Project { get; set; }
        public User User { get; set; }
    }
}
