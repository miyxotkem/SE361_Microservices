using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Contents.AddCourseContent
{
    public class AddCourseContentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses/{courseId}/contents", async (string courseId, CreateCourseContentRequest req, ISender sender) =>
            {
                var cmd = new AddCourseContentCommand(courseId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
