// SignalR connection management - Simplified Version
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Online users tracking
let onlineUsers = new Set();

// Start the connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");

        // Request current online users list
        await connection.invoke("GetOnlineUsers");

    } catch (err) {
        console.error("SignalR connection failed:", err);
        setTimeout(startConnection, 5000);
    }
}

// Handle incoming notifications
connection.on("ReceiveNotification", (message, newRole) => {
    console.log(newRole);
    console.log(message);
    showNotification(message, 'info');
});

connection.on("TaskUpdated", (taskId) => {
    showNotification("A task has been updated", 'info');
});

connection.on("ProjectUpdated", (projectId) => {
    showNotification("A project has been updated", 'info');
});

// Online users handling
connection.on("UserOnline", (userId) => {
    onlineUsers.add(userId);
    updateOnlineUsersDisplay();
});

connection.on("UserOffline", (userId) => {
    onlineUsers.delete(userId);
    updateOnlineUsersDisplay();
});

connection.on("OnlineUsersList", (users) => {
    onlineUsers = new Set(users);
    updateOnlineUsersDisplay();
});

// Update online users display
function updateOnlineUsersDisplay() {
    const onlineIndicator = document.getElementById('online-users-count');
    if (onlineIndicator) {
        const count = onlineUsers.size;
        onlineIndicator.textContent = `${count} user${count !== 1 ? 's' : ''} online`;
    }
}

// Basic notification function
function showNotification(message, type = 'info') {
    console.log(`Notification (${type}):`, message);

    

    // Simple browser notification (you can enhance this later)
    if (type === 'info' && 'Notification' in window && Notification.permission === 'granted') {
        new Notification('Task Management', { body: message });
    }
}

// Connection status handling
connection.onclose(() => {
    console.log("SignalR connection closed");
});

connection.onreconnecting(() => {
    console.log("SignalR reconnecting...");
});

connection.onreconnected(() => {
    console.log("SignalR reconnected");
    connection.invoke("GetOnlineUsers");
});


// Handle menu update requests
connection.on("SendMenuUpdate", (newRole) => {
    console.log('The updated role is ' + newRole);
    updateMenuBasedOnRole(newRole);
});

// Function to update menu based on specific role
function updateMenuBasedOnRole(newRole) {
    const menuContainer = document.querySelector('.navbar-nav.me-auto');
    if (!menuContainer) return;

    // Clear existing menu items
    menuContainer.innerHTML = '';

    // Add menu items based on the new role
    if (newRole === 'Admin' || newRole === 'Manager') {
        menuContainer.innerHTML += `
            <li class="nav-item">
                <a class="nav-link ${window.location.pathname.startsWith('/Projects') ? 'active' : ''}"
                   href="/Projects/Index">
                    <i class="bi bi-folder me-1"></i>Projects
                </a>
            </li>
        `;
    }

    // Always show Tasks for Admin, Manager, and Member
    if (newRole === 'Admin' || newRole === 'Manager' || newRole === 'Member') {
        menuContainer.innerHTML += `
            <li class="nav-item">
                <a class="nav-link ${window.location.pathname.startsWith('/Tasks') ? 'active' : ''}"
                   href="/Tasks/Index">
                    <i class="bi bi-list-task me-1"></i>Tasks
                </a>
            </li>
        `;
    }

    // Admin-only menu items
    if (newRole === 'Admin') {
        menuContainer.innerHTML += `
            <li class="nav-item">
                <a class="nav-link ${window.location.pathname.startsWith('/Admin/Users') ? 'active' : ''}"
                   href="/Admin/Users/Index">
                    <i class="bi bi-people me-1"></i>User Management
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link ${window.location.pathname.startsWith('/Admin/AuditLogs') ? 'active' : ''}"
                   href="/Admin/AuditLogs/Index">
                    <i class="bi bi-clipboard-data me-1"></i>Audit Logs
                </a>
            </li>
        `;
    }

    console.log('Menu updated for role:', newRole);
}

// Start the connection when page loads
document.addEventListener('DOMContentLoaded', function () {
    // Request notification permission
    if ('Notification' in window) {
        Notification.requestPermission();
    }

    startConnection();
});

// Export for global access
window.signalRConnection = connection;