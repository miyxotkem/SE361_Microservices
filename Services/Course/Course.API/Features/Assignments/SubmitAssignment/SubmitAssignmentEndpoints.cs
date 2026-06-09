using Carter;
using Course.API.Models;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Assignments.SubmitAssignment
{
    public class SubmitAssignmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses/{courseId}/assignments/{asmId}/submit", async (string courseId, string asmId, SubmitAssignmentRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in token");
                var cmd = new SubmitAssignmentCommand(courseId, asmId, uid, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Student", "Instructor", "Admin"));
        }
    }
}
