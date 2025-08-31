using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private AuditLogRepository _auditLogRepository;
        private ProjectRepository _projectRepository;
        private ProjectAssignmentRepository _projectAssignmentRepository;
        private TaskItemRepository _taskItemRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_context);
        public IProjectRepository Projects => _projectRepository ??= new ProjectRepository(_context);
        public IProjectAssignmentRepository ProjectAssignments => _projectAssignmentRepository ??= new ProjectAssignmentRepository(_context);
        public ITaskItemRepository Tasks => _taskItemRepository ??= new TaskItemRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
