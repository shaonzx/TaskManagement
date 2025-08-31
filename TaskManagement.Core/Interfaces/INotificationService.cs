using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskAssignedAsync(TaskItem task, string assignedById);
        Task NotifyTaskUpdatedAsync(TaskItem task, string updatedById);
        Task NotifyTaskStatusChangedAsync(TaskItem task, string oldStatus, string updatedById); // Add this line
        Task NotifyProjectUpdatedAsync(Project project, string updatedById);
        Task NotifyRoleChangedAsync(ApplicationUser user, string oldRole, string newRole, string changedById);
    }
}