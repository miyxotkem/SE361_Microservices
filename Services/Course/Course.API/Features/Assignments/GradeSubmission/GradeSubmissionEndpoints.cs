using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Assignments.GradeSubmission
{
    public class GradeSubmissionEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/assignments/{asmId}/submissions/{studentId}/grade", async (string courseId, string asmId, string studentId, GradeSubmissionRequest req, ISender sender) =>
            {
                var cmd = new GradeSubmissionCommand(courseId, asmId, studentId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
