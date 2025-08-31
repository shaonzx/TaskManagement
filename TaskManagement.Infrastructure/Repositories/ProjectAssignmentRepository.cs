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
    public class ProjectAssignmentRepository : Repository<ProjectAssignment>, IProjectAssignmentRepository
    {
        public ProjectAssignmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectIdAsync(int projectId)
        {
            return await _context.ProjectAssignments
                .Include(pa => pa.User)
                .Include(pa => pa.AssignedBy)
                .Where(pa => pa.ProjectId == projectId && pa.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByUserIdAsync(string userId)
        {
            return await _context.ProjectAssignments
                .Include(pa => pa.Project)
                .Include(pa => pa.AssignedBy)
                .Where(pa => pa.UserId == userId && pa.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsUserAssignedToProjectAsync(string userId, int projectId)
        {
            return await _context.ProjectAssignments
                .AnyAsync(pa => pa.UserId == userId && pa.ProjectId == projectId && pa.IsActive);
        }
    }
}
