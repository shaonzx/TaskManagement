using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Pages
{
    public class RefreshRolesModel : BasePageModel
    {
        public async void OnGet()
        {
            await InitializeUserProperties();
        }

        public RefreshRolesModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IRbacService rbacService, ILogger<BasePageModel> logger) 
            : base(unitOfWork, userManager, rbacService, logger)
        {
        }
    }
}
