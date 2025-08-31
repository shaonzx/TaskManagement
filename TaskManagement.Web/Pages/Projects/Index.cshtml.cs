using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Projects
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
            IRbacService rbacService, ILogger<BasePageModel> logger)
            : base(unitOfWork, userManager, rbacService, logger)
        {
        }

        public List<Core.Entities.Project> Projects { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeUserProperties();

            if (IsAdmin)
            {
                Projects = (await _unitOfWork.Projects.GetProjectsWithDetailsAsync()).ToList();
            }
            else
            {
                //Projects = (await _unitOfWork.Projects.GetProjectsByUserIdAsync(CurrentUserId)).ToList();
                Projects = (await _unitOfWork.Projects.GetProjectsByAssignedUserIdAsync(CurrentUserId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.IsActive = false;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.CompleteAsync();

            AddAuditLog("Project Deleted", $"Project '{project.Name}' deleted");
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Project '{project.Name}' deleted successfully!";
            return RedirectToPage();
        }
    }
}