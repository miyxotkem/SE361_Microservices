using Carter;
using MediatR;

namespace Course.API.Features.Assignments.GetAssignments
{
    public class GetAssignmentsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/assignments", async (string courseId, ISender sender) =>
            {
                var query = new GetAssignmentsQuery(courseId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
