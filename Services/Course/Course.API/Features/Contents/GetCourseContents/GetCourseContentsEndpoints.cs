using Carter;
using MediatR;

namespace Course.API.Features.Contents.GetCourseContents
{
    public class GetCourseContentsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/contents", async (string courseId, ISender sender) =>
            {
                var query = new GetCourseContentsQuery(courseId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
