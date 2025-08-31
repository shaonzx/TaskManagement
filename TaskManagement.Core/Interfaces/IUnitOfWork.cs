using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAuditLogRepository AuditLogs { get; }
        IProjectRepository Projects { get; }
        IProjectAssignmentRepository ProjectAssignments { get; }
        ITaskItemRepository Tasks { get; }
        Task<int> CompleteAsync();
    }
}
