using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            return await _context.Projects
                .Where(p => p.ProjectAssignments.Any(pa => pa.UserId == userId && pa.IsActive) || p.CreatedById == userId)
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.User)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByAssignedUserIdAsync(string assignedUserId)
        {
            return await _context.Projects
                .Where(p => p.AssignedToId != null && p.AssignedToId.Equals(assignedUserId))
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.User)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsWithDetailsAsync()
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.AssignedTo)
                .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.User)
                .Include(p => p.Tasks)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Project> GetProjectWithDetailsAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.User)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }
    }
}
