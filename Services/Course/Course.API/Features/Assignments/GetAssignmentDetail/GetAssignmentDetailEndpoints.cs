using Carter;
using MediatR;

namespace Course.API.Features.Assignments.GetAssignmentDetail
{
    public class GetAssignmentDetailEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/assignments/{asmId}", async (string courseId, string asmId, ISender sender) =>
            {
                var query = new GetAssignmentDetailQuery(courseId, asmId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
