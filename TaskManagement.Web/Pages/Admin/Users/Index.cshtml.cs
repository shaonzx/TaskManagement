using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Admin.Users
{
    public class IndexModel : BasePageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotificationService _notificationService;

        public IndexModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                         IRbacService rbacService, ILogger<BasePageModel> logger,
                         RoleManager<IdentityRole> roleManager, INotificationService notificationService)
            : base(unitOfWork, userManager, rbacService, logger)
        {
            _roleManager = roleManager;
            _notificationService = notificationService;
        }

        public List<ApplicationUser> Users { get; set; }
        public Dictionary<string, List<string>> UserRoles { get; set; } = new();

        [BindProperty]
        public RegisterModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }

            await InitializePageData();
            return Page();
        }

        private async Task InitializePageData()
        {
            Users = await _userManager.Users.ToListAsync();

            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.ToList();
            }
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var oldRole = currentRoles.FirstOrDefault();

            // Remove all current roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new role
            await _userManager.AddToRoleAsync(user, newRole);

            // Send notification
            await _notificationService.NotifyRoleChangedAsync(user, oldRole ?? "None", newRole, CurrentUserId);

            TempData["SuccessMessage"] = $"Successfully changed {user.FirstName}'s role to {newRole}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await InitializePageData();
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                EmailConfirmed = true // Auto-confirm for admin-created users
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Assign the selected role
                await _userManager.AddToRoleAsync(user, Input.SelectedRole);

                // Log the user creation
                AddAuditLog("User Created", $"Admin created user: {Input.Email} with role: {Input.SelectedRole}");
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = $"User {Input.Email} created successfully!";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await InitializePageData();
            return Page();
        }

        public class RegisterModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Role")]
            public string SelectedRole { get; set; }
        }
    }
}