using Carter;
using MediatR;

namespace Course.API.Features.Contents.DeleteCourseContent
{
    public class DeleteCourseContentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{courseId}/contents/{contentId}", async (string courseId, string contentId, ISender sender) =>
            {
                var cmd = new DeleteCourseContentCommand(courseId, contentId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
