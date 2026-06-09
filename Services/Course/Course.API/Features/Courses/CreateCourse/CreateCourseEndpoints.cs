using Carter;
using Course.API.Models;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Courses.CreateCourse
{
    public class CreateCourseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses", async (CreateCourseRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                if (string.IsNullOrEmpty(req.InstructorId))
                {
                    req.InstructorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                }
                var cmd = new CreateCourseCommand(req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
