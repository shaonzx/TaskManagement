using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId);
        Task<IEnumerable<Project>> GetProjectsByAssignedUserIdAsync(string assignedUserId);
        Task<IEnumerable<Project>> GetProjectsWithDetailsAsync();
        Task<Project> GetProjectWithDetailsAsync(int id);
    }
}
