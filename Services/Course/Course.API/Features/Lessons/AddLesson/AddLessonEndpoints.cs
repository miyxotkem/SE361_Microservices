using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Lessons.AddLesson
{
    public class AddLessonEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/courses/{courseId}/lessons", async (string courseId, CreateLessonRequest req, ISender sender) =>
            {
                var cmd = new AddLessonCommand(courseId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
