using Carter;
using MediatR;

namespace Course.API.Features.Assignments.PublishGrades
{
    public class PublishGradesEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/assignments/{asmId}/publish-grades", async (string courseId, string asmId, ISender sender) =>
            {
                var cmd = new PublishGradesCommand(courseId, asmId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
