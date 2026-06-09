using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Contents.UpdateCourseContent
{
    public class UpdateCourseContentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/contents/{contentId}", async (string courseId, string contentId, UpdateCourseContentRequest req, ISender sender) =>
            {
                var cmd = new UpdateCourseContentCommand(courseId, contentId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
