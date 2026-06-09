using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Assignments.UpdateAssignment
{
    public class UpdateAssignmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/assignments/{asmId}", async (string courseId, string asmId, UpdateAssignmentRequest req, ISender sender) =>
            {
                var cmd = new UpdateAssignmentCommand(courseId, asmId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
