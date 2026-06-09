using Carter;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Registrations.RegisterCourse
{
    public class RegisterCourseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses/{courseId}/register", async (string courseId, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in token");
                var cmd = new RegisterCourseCommand(courseId, uid);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
