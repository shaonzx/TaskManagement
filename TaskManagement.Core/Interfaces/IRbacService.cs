using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface IRbacService
    {
        Task<bool> IsUserInRoleAsync(string userId, RoleType role);
        Task<bool> IsUserAdminAsync(string userId);
        Task<bool> IsUserManagerAsync(string userId);
        Task<bool> IsUserMemberAsync(string userId);
        Task<RoleType> GetUserRoleAsync(string userId);
        Task<bool> CanUserAccessProjectAsync(string userId, int projectId);
        Task<bool> CanUserModifyTaskAsync(string userId, int taskId);
        Task<bool> CanUserViewTaskAsync(string userId, int taskId);
    }
}
