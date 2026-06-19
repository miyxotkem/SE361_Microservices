using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Course.API.Hubs;

public class EnrollmentHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();

    public override Task OnConnectedAsync()
    {
        // Require UserId via query string for mapping
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.TryRemove(userId, out _);
        }

        return base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(string userId)
    {
        _userConnections.TryGetValue(userId, out var connectionId);
        return connectionId;
    }
}
