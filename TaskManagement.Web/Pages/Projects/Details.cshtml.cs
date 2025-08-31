using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Projects
{
    public class DetailsModel : BasePageModel
    {
        private readonly INotificationService _notificationService;

        public DetailsModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                          IRbacService rbacService, ILogger<BasePageModel> logger,
                          INotificationService notificationService)
            : base(unitOfWork, userManager, rbacService, logger)
        {
            _notificationService = notificationService;
        }

        public Core.Entities.Project Project { get; set; }
        public List<Core.Entities.TaskItem> Tasks { get; set; }
        public List<Core.Entities.ProjectAssignment> Assignments { get; set; }
        
        public async Task<IActionResult> OnPostAddAssignmentAsync(int projectId, string userId)
        {
            if (!IsAdmin && !IsManager)
            {
                return Forbid();
            }

            if (!await _rbacService.CanUserAccessProjectAsync(CurrentUserId, projectId))
            {
                return Forbid();
            }

            var assignment = new Core.Entities.ProjectAssignment
            {
                ProjectId = projectId,
                UserId = userId,
                AssignedById = CurrentUserId,
                AssignedDate = DateTime.UtcNow
            };

            await _unitOfWork.ProjectAssignments.AddAsync(assignment);
            await _unitOfWork.CompleteAsync();

            // Notify the assigned user
            var user = await _userManager.FindByIdAsync(userId);
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);

            await _notificationService.NotifyProjectUpdatedAsync(project, CurrentUserId);

            TempData["SuccessMessage"] = $"{user.FirstName} has been added to the project";
            return RedirectToPage(new { id = projectId });
        }

        public async Task<IActionResult> OnPostRemoveAssignmentAsync(int assignmentId)
        {
            var assignment = await _unitOfWork.ProjectAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            if (!IsAdmin && !IsManager)
            {
                return Forbid();
            }

            if (!await _rbacService.CanUserAccessProjectAsync(CurrentUserId, assignment.ProjectId))
            {
                return Forbid();
            }

            assignment.IsActive = false;
            _unitOfWork.ProjectAssignments.Update(assignment);
            await _unitOfWork.CompleteAsync();

            // Notify project members
            var project = await _unitOfWork.Projects.GetByIdAsync(assignment.ProjectId);
            await _notificationService.NotifyProjectUpdatedAsync(project, CurrentUserId);

            TempData["SuccessMessage"] = "User has been removed from the project";
            return RedirectToPage(new { id = assignment.ProjectId });
        }

        // Add to the DetailsModel class
        public SelectList AvailableUsers { get; set; }

        // Update OnGetAsync method
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Project = await _unitOfWork.Projects.GetProjectWithDetailsAsync(id);

            if (Project == null)
            {
                return NotFound();
            }

            if (!await _rbacService.CanUserAccessProjectAsync(CurrentUserId, id))
            {
                return Forbid();
            }

            Tasks = Project.Tasks.Where(t => t.IsActive).ToList();
            Assignments = Project.ProjectAssignments.Where(pa => pa.IsActive).ToList();

            // Get users not already assigned to the project
            var assignedUserIds = Assignments.Select(a => a.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => u.IsActive && !assignedUserIds.Contains(u.Id))
                .ToList();

            AvailableUsers = new SelectList(availableUsers, "Id", "FirstName");
            ViewData["AvailableUsers"] = AvailableUsers;

            return Page();
        }

        // Add this method to the DetailsModel class
        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin && !IsManager)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadProjectData();
                return Page();
            }

            var task = new Core.Entities.TaskItem
            {
                Title = TaskInput.Title,
                Description = TaskInput.Description,
                ProjectId = Project.Id,
                AssignedToId = TaskInput.AssignedToId,
                Priority = TaskInput.Priority,
                DueDate = TaskInput.DueDate,
                CreatedById = CurrentUserId,
                Status = Core.Enums.TaskStatus.Pending,
                IsActive = true
            };

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.CompleteAsync();

            // Send notification to assigned user
            await _notificationService.NotifyTaskAssignedAsync(task, CurrentUserId);

            TempData["SuccessMessage"] = $"Task '{task.Title}' created successfully!";
            return RedirectToPage(new { id = Project.Id });
        }

        // Add this class inside DetailsModel class
        public class TaskInputModel
        {
            [Required]
            public string Title { get; set; }

            public string Description { get; set; }

            [Required]
            public string AssignedToId { get; set; }

            public Core.Enums.TaskPriority Priority { get; set; } = Core.Enums.TaskPriority.Medium;

            public DateTime? DueDate { get; set; }
        }

        [BindProperty]
        public TaskInputModel TaskInput { get; set; }

        // Update OnGetAsync to load available users
        private async Task LoadProjectData()
        {
            Project = await _unitOfWork.Projects.GetProjectWithDetailsAsync(Project.Id);
            Tasks = Project.Tasks.Where(t => t.IsActive).ToList();
            Assignments = Project.ProjectAssignments.Where(pa => pa.IsActive).ToList();

            // Get users assigned to this project for task assignment
            var assignedUserIds = Assignments.Select(a => a.UserId).ToList();
            var availableUsers = _userManager.Users
                .Where(u => u.IsActive && assignedUserIds.Contains(u.Id))
                .ToList();

            AvailableUsers = new SelectList(availableUsers, "Id", "FirstName");
            ViewData["AvailableUsers"] = AvailableUsers;
        }
    }
}