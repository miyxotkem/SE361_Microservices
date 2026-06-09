using Carter;
using MediatR;
using System.Security.Claims;

namespace Notification.API.Features.Notifications.GetNotifications
{
    public class GetNotificationsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/notifications", async (ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var query = new GetNotificationsQuery(uid);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
