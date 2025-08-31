using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public string Details { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
