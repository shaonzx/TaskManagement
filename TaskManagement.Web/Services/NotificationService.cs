using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Web.Hubs;

namespace TaskManagement.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRbacService _rbacService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(IHubContext<NotificationHub> hubContext, IUnitOfWork unitOfWork, IRbacService rbacService, UserManager<ApplicationUser> userManager)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
            _rbacService = rbacService;
            _userManager = userManager;
        }

        public async Task NotifyTaskAssignedAsync(TaskItem task, string assignedById)
        {
            // Notify the assigned user
            await _hubContext.Clients.User(task.AssignedToId).SendAsync("ReceiveNotification",
                $"New task assigned: {task.Title}");

            // Log the assignment - FIXED: Get the user who assigned the task
            var assignedByUser = await _userManager.FindByIdAsync(assignedById);
            var details = $"Task '{task.Title}' assigned to {task.AssignedTo?.FirstName} by {assignedByUser?.FirstName}";

            var auditLog = new AuditLog
            {
                Action = "Task Assigned",
                Details = details,
                UserId = assignedById,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public async Task NotifyTaskUpdatedAsync(TaskItem task, string updatedById)
        {
            // Notify all users in the project group
            await _hubContext.Clients.Group($"project-{task.ProjectId}")
                .SendAsync("TaskUpdated", task.Id);

            // Log the update
            var details = $"Task '{task.Title}' updated by {updatedById}";
            var auditLog = new AuditLog
            {
                Action = "Task Updated",
                Details = details,
                UserId = updatedById,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public async Task NotifyTaskStatusChangedAsync(TaskItem task, string oldStatus, string updatedById)
        {
            // Notify all users in the project group
            await _hubContext.Clients.Group($"project-{task.ProjectId}")
                .SendAsync("TaskStatusUpdated", task.Id, task.Status.ToString());

            // Log the status change
            var details = $"Task '{task.Title}' status changed from {oldStatus} to {task.Status} by {updatedById}";
            var auditLog = new AuditLog
            {
                Action = "Task Status Changed",
                Details = details,
                UserId = updatedById,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public async Task NotifyProjectUpdatedAsync(Project project, string updatedById)
        {
            // Notify all users assigned to the project
            await _hubContext.Clients.Group($"project-{project.Id}").SendAsync("ProjectUpdated", project.Id);

            // Log the update
            var details = $"Project '{project.Name}' updated by {updatedById}";
            var auditLog = new AuditLog
            {
                Action = "Project Updated",
                Details = details,
                UserId = updatedById,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public async Task NotifyRoleChangedAsync(ApplicationUser user, string oldRole, string newRole, string changedById)
        {
            // Notify the user whose role was changed
            await _hubContext.Clients.User(user.Id).SendAsync("ReceiveNotification",
                $"Your role has been changed from {oldRole} to {newRole}. Please refresh the page for changes to take effect.");

            // Log the role change
            var details = $"{user.FirstName} {user.LastName}'s role changed from {oldRole} to {newRole} by {changedById}";
            var auditLog = new AuditLog
            {
                Action = "Role Changed",
                Details = details,
                UserId = changedById,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();


            // Notify the specific user that their menu needs to be updated
            await _hubContext.Clients.User(user.Id).SendAsync("SendMenuUpdate", newRole);
        }
    }
}