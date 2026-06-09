using Carter;
using MediatR;
using System.Security.Claims;

namespace Notification.API.Features.Notifications.MarkAsRead
{
    public class MarkAsReadEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/notifications/read", async (MarkReadRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new MarkAsReadCommand(uid, req.NotificationIds);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }

        public class MarkReadRequest
        {
            public List<string> NotificationIds { get; set; } = new();
        }
    }
}
