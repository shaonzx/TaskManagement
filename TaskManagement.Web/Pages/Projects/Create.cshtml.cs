using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Projects
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
        public Core.Entities.Project Project { get; set; }
        public SelectList Users { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }
            await InitializeSelectLists();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }
            Project.CreatedById = CurrentUserId;
            Project.CreatedDate = DateTime.UtcNow;
            Project.IsActive = true;

            await _unitOfWork.Projects.AddAsync(Project);
            await _unitOfWork.CompleteAsync();

            // Auto-assign the creator to the project
            var assignment = new Core.Entities.ProjectAssignment
            {
                ProjectId = Project.Id,
                UserId = CurrentUserId,
                AssignedById = CurrentUserId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.ProjectAssignments.AddAsync(assignment);
            await _unitOfWork.CompleteAsync();

            AddAuditLog("Project Created", $"Project '{Project.Name}' created");
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Project '{Project.Name}' created successfully!";
            return RedirectToPage("./Index");
        }

        private async Task InitializeSelectLists()
        {

            List<ApplicationUser> managers = new List<ApplicationUser>();
            var allUsers = _userManager.Users.Where(u => u.IsActive).ToList();
            foreach (var user in allUsers)
            {
                var role = await _userManager.GetRolesAsync(user);
                if (role.Contains("Manager"))
                {
                    managers.Add(user);
                }
            }
            Users = new SelectList(managers, "Id", "FirstName");

            /*var users = _userManager.Users
                .Where(u => u.IsActive && u..Roles.Any(r => r.Name == "Manager"))
                .ToList();*/
        }
    }
}