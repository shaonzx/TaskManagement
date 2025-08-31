using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Tasks
{
    public class EditModel : BasePageModel
    {
        private readonly INotificationService _notificationService;

        public EditModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                       IRbacService rbacService, ILogger<BasePageModel> logger,
                       INotificationService notificationService)
            : base(unitOfWork, userManager, rbacService, logger)
        {
            _notificationService = notificationService;
        }

        [BindProperty]
        public Core.Entities.TaskItem TaskItem { get; set; }

        public SelectList StatusList { get; set; }
        public SelectList PriorityList { get; set; }
        public SelectList Users { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await InitializeUserProperties();

            TaskItem = await _unitOfWork.Tasks.GetByIdAsync(id);

            if (TaskItem == null)
            {
                return NotFound();
            }

            if (!(IsManager || IsMember))
            {
                return Forbid();
            }

            await InitializeSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await _rbacService.CanUserModifyTaskAsync(CurrentUserId, TaskItem.Id))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await InitializeSelectLists();
                return Page();
            }

            var existingTask = await _unitOfWork.Tasks.GetByIdAsync(TaskItem.Id);

            // Track changes for notification
            var oldStatus = existingTask.Status;
            var oldAssignee = existingTask.AssignedToId;

            // Update properties
            existingTask.Title = TaskItem.Title;
            existingTask.Description = TaskItem.Description;
            existingTask.Status = TaskItem.Status;
            existingTask.Priority = TaskItem.Priority;
            existingTask.DueDate = TaskItem.DueDate;
            existingTask.AssignedToId = TaskItem.AssignedToId;

            if (TaskItem.Status == Core.Enums.TaskStatus.Completed && existingTask.CompletedDate == null)
            {
                existingTask.CompletedDate = DateTime.UtcNow;
            }
            else if (TaskItem.Status != Core.Enums.TaskStatus.Completed)
            {
                existingTask.CompletedDate = null;
            }

            _unitOfWork.Tasks.Update(existingTask);
            await _unitOfWork.CompleteAsync();

            // Send notifications for important changes
            if (oldStatus != existingTask.Status)
            {
                await _notificationService.NotifyTaskUpdatedAsync(existingTask, CurrentUserId);
            }

            if (oldAssignee != existingTask.AssignedToId)
            {
                await _notificationService.NotifyTaskAssignedAsync(existingTask, CurrentUserId);
            }

            return RedirectToPage("./Index");
        }

        private async Task InitializeSelectLists()
        {
            StatusList = new SelectList(Enum.GetValues(typeof(Core.Enums.TaskStatus)));
            PriorityList = new SelectList(Enum.GetValues(typeof(Core.Enums.TaskPriority)));

            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            Users = new SelectList(users, "Id", "FirstName", TaskItem.AssignedToId);
        }
    }
}