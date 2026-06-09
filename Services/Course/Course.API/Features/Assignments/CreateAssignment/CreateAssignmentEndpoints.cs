using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Assignments.CreateAssignment
{
    public class CreateAssignmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses/{courseId}/assignments", async (string courseId, CreateAssignmentRequest req, ISender sender) =>
            {
                var cmd = new CreateAssignmentCommand(courseId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
