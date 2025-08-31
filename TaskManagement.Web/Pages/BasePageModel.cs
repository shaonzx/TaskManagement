using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Web.Services;

namespace TaskManagement.Web.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IRbacService _rbacService;
        protected readonly ILogger<BasePageModel> _logger;

        public BasePageModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                           IRbacService rbacService, ILogger<BasePageModel> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _rbacService = rbacService;
            _logger = logger;
        }

        protected string CurrentUserId => _userManager.GetUserId(User);
        protected async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(User);

        // Public properties for access in Razor views
        public bool IsAdmin { get; protected set; }
        public bool IsManager { get; protected set; }
        public bool IsMember { get; protected set; }
        public string CurrentUserRole { get; protected set; }

        // Remove the OnGetAsync and OnPostAsync methods from here
        // They should only be in the specific page models

        protected async Task InitializeUserProperties()
        {
            IsAdmin = await _rbacService.IsUserAdminAsync(CurrentUserId);
            IsManager = await _rbacService.IsUserManagerAsync(CurrentUserId);
            IsMember = await _rbacService.IsUserMemberAsync(CurrentUserId);
            CurrentUserRole = (await _rbacService.GetUserRoleAsync(CurrentUserId)).ToString();
        }

        protected void AddAuditLog(string action, string details)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                Details = details,
                UserId = CurrentUserId,
                Timestamp = DateTime.UtcNow
            };

            _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
    }
}