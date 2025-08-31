using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Tasks
{
    public class CreateModel : BasePageModel
    {
        private readonly INotificationService _notificationService;

        public CreateModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                         IRbacService rbacService, ILogger<BasePageModel> logger,
                         INotificationService notificationService)
            : base(unitOfWork, userManager, rbacService, logger)
        {
            _notificationService = notificationService;
        }

        [BindProperty]
        public Core.Entities.TaskItem TaskItem { get; set; }

        public SelectList Projects { get; set; }
        public SelectList Users { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId = null)
        {
            await InitializeUserProperties();

            if (!IsAdmin && !IsManager)
            {
                return Forbid();
            }

            await InitializeSelectLists();

            // Pre-select project if provided
            if (projectId.HasValue)
            {
                TaskItem = new Core.Entities.TaskItem { ProjectId = projectId.Value };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin && !IsManager)
            {
                return Forbid();
            }

            // Set additional task properties
            TaskItem.CreatedById = CurrentUserId;
            TaskItem.CreatedDate = DateTime.UtcNow;
            TaskItem.Status = Core.Enums.TaskStatus.Pending;
            TaskItem.IsActive = true;

            await _unitOfWork.Tasks.AddAsync(TaskItem);
            await _unitOfWork.CompleteAsync();

            // Get the complete task with related data for notification
            var createdTask = await _unitOfWork.Tasks.GetByIdAsync(TaskItem.Id);
            if (createdTask != null)
            {
                // Send notification to assigned user
                await _notificationService.NotifyTaskAssignedAsync(createdTask, CurrentUserId);
            }

            TempData["SuccessMessage"] = $"Task '{TaskItem.Title}' created successfully!";
            return RedirectToPage("./Index");
        }

        private async Task InitializeSelectLists()
        {
            // Get projects user has access to
            List<Core.Entities.Project> projects;
            if (IsAdmin)
            {
                projects = (await _unitOfWork.Projects.GetAllAsync()).Where(p => p.IsActive).ToList();
            }
            else
            {
                projects = (await _unitOfWork.Projects.GetProjectsByAssignedUserIdAsync(CurrentUserId)).Where(p => p.IsActive).ToList();
            }

            Projects = new SelectList(projects, "Id", "Name");

            // Get members for assignment
            List<ApplicationUser> members = new List<ApplicationUser>();
            var allUsers = _userManager.Users.Where(u => u.IsActive).ToList();
            foreach (var user in allUsers)
            {
                var role = await _userManager.GetRolesAsync(user);
                if (role.Contains("Member"))
                {
                    members.Add(user);
                }
            }
            Users = new SelectList(members, "Id", "FirstName");
        }
    }
}