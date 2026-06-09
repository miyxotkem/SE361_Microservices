using Carter;
using MediatR;

namespace Course.API.Features.Assignments.DeleteAssignment
{
    public class DeleteAssignmentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{courseId}/assignments/{asmId}", async (string courseId, string asmId, ISender sender) =>
            {
                var cmd = new DeleteAssignmentCommand(courseId, asmId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
