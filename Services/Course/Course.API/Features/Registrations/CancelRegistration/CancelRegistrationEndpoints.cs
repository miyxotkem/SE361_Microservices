using Carter;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Registrations.CancelRegistration
{
    public class CancelRegistrationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{courseId}/register", async (string courseId, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in token");
                var cmd = new CancelRegistrationCommand(courseId, uid);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
