using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IProjectAssignmentRepository : IRepository<ProjectAssignment>
    {
        Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectIdAsync(int projectId);
        Task<IEnumerable<ProjectAssignment>> GetAssignmentsByUserIdAsync(string userId);
        Task<bool> IsUserAssignedToProjectAsync(string userId, int projectId);
    }
}
