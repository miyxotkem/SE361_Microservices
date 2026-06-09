using Carter;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Assignments.GetSubmissions
{
    public class GetSubmissionsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/assignments/{asmId}/submissions", async (string courseId, string asmId, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in token");
                bool isStudent = user.IsInRole("Student");
                var query = new GetSubmissionsQuery(courseId, asmId, uid, isStudent);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
