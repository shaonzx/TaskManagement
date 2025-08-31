using Microsoft.AspNetCore.Identity;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Web.Services
{
    public class RbacService : IRbacService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public RbacService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsUserInRoleAsync(string userId, RoleType role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var roleName = role.ToString();
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserAdminAsync(string userId)
        {
            return await IsUserInRoleAsync(userId, RoleType.Admin);
        }

        public async Task<bool> IsUserManagerAsync(string userId)
        {
            return await IsUserInRoleAsync(userId, RoleType.Manager);
        }

        public async Task<bool> IsUserMemberAsync(string userId)
        {
            return await IsUserInRoleAsync(userId, RoleType.Member);
        }

        public async Task<RoleType> GetUserRoleAsync(string userId)
        {
            if (await IsUserAdminAsync(userId)) return RoleType.Admin;
            if (await IsUserManagerAsync(userId)) return RoleType.Manager;
            if (await IsUserMemberAsync(userId)) return RoleType.Member;

            return RoleType.Member; // Default role
        }

        public async Task<bool> CanUserAccessProjectAsync(string userId, int projectId)
        {
            // Admin can access all projects
            if (await IsUserAdminAsync(userId)) return true;

            // Check if user is assigned to the project
            return await _unitOfWork.ProjectAssignments.IsUserAssignedToProjectAsync(userId, projectId);
        }

        public async Task<bool> CanUserModifyTaskAsync(string userId, int taskId)
        {
            // Admin can modify any task
            if (await IsUserAdminAsync(userId)) return true;

            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null) return false;

            // Manager can modify tasks in their projects
            if (await IsUserManagerAsync(userId))
            {
                return await CanUserAccessProjectAsync(userId, task.ProjectId);
            }

            // Members can only modify their own tasks
            return task.AssignedToId == userId;
        }

        public async Task<bool> CanUserViewTaskAsync(string userId, int taskId)
        {
            // Admin can view any task
            if (await IsUserAdminAsync(userId)) return true;

            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null) return false;

            // Check if user has access to the project
            return await CanUserAccessProjectAsync(userId, task.ProjectId);
        }
    }
}
