using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public string? AssignedToId { get; set; }
        public ApplicationUser AssignedTo { get; set; }


        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }
}
