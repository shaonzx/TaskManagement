using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TaskManagement.Web.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // concurrent dictionary for tracking connections
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;

                // Notify all users about new connection
                await Clients.All.SendAsync("UserOnline", userId);

                // Send current online users to the new client
                await Clients.Caller.SendAsync("OnlineUsersList", _userConnections.Keys.ToList());

                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId) && _userConnections.TryRemove(userId, out _))
            {
                // Notify all users about disconnection
                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method to get current online users
        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(_userConnections.Keys.ToList());
        }

        // Basic notification methods
        public async Task NotifyTaskAssignment(string taskId, string assignedToUserId, string taskTitle)
        {
            await Clients.User(assignedToUserId).SendAsync("ReceiveNotification",
                $"New task assigned: {taskTitle}");
        }

        public async Task NotifyTaskUpdate(string taskId, string projectId)
        {
            await Clients.Group($"project-{projectId}").SendAsync("TaskUpdated", taskId);
        }

        public async Task NotifyProjectUpdate(string projectId)
        {
            await Clients.Group($"project-{projectId}").SendAsync("ProjectUpdated", projectId);
        }

        // Group management
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }

        // dynamic menu update
        public async Task SendMenuUpdate(string userId)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("SendMenuUpdate");
            }
        }
    }
}