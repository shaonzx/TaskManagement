using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Entities
{
    public class ProjectAssignment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public string AssignedById { get; set; }
        public ApplicationUser AssignedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
