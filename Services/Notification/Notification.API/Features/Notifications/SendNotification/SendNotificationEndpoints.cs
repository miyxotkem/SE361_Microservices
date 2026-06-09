using Carter;
using MediatR;
using System.Security.Claims;

namespace Notification.API.Features.Notifications.SendNotification
{
    public class SendNotificationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/notifications/send", async (SendNotificationRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new SendNotificationCommand(uid, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin", "Student"));
        }
    }
}
