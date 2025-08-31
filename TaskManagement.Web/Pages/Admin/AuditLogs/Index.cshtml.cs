using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages.Admin.AuditLogs
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

        public List<Core.Entities.AuditLog> AuditLogs { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeUserProperties();

            if (!IsAdmin)
            {
                return Forbid();
            }

            AuditLogs = (await _unitOfWork.AuditLogs.GetAllAsync())
                .OrderByDescending(al => al.Timestamp)
                .ToList();

            return Page();
        }
    }
}