using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Tasks
{
    public class IndexModel : BasePageModel
    {
        private readonly INotificationService _notificationService;

        public IndexModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                         IRbacService rbacService, ILogger<BasePageModel> logger, INotificationService notificationService)
            : base(unitOfWork, userManager, rbacService, logger)
        {
            _notificationService = notificationService;
        }

        public List<Core.Entities.TaskItem> Tasks { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeUserProperties();

            Tasks = new List<Core.Entities.TaskItem>();
            if (IsAdmin)
            {
                Tasks = (await _unitOfWork.Tasks.GetTasksWithDetailsAsync()).ToList();
            }
            else if (IsManager)
            {
                // Managers see tasks from projects they're assigned to
                var userProjects = (await _unitOfWork.Projects.GetProjectsByAssignedUserIdAsync(CurrentUserId)).ToList();
                var allTasks = await _unitOfWork.Tasks.GetTasksWithDetailsAsync();
                foreach (var proj in userProjects)
                {
                    var assignedTasks = allTasks.Where(t => t.ProjectId == proj.Id).ToList();
                    Tasks.AddRange(assignedTasks);
                }
            }
            else
            {
                // Members see only their own tasks
                Tasks = (await _unitOfWork.Tasks.GetTasksByUserIdAsync(CurrentUserId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int taskId, string status)
        {
            await InitializeUserProperties();

            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
            {
                return NotFound();
            }

            if (!(IsManager || IsMember))
            {
                return Forbid();
            }

            var oldStatus = task.Status;
            task.Status = Enum.Parse<Core.Enums.TaskStatus>(status);

            if (task.Status == Core.Enums.TaskStatus.Completed)
            {
                task.CompletedDate = DateTime.UtcNow;
            }
            else
            {
                task.CompletedDate = null;
            }

            _unitOfWork.Tasks.Update(task);
            await _unitOfWork.CompleteAsync();

            // Send notification
            await _notificationService.NotifyTaskStatusChangedAsync(task, oldStatus.ToString(), CurrentUserId);

            TempData["SuccessMessage"] = $"Task status updated to {task.Status}";
            return RedirectToPage();
        }
    }
}