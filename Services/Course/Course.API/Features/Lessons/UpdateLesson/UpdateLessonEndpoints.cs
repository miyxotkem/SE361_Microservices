using Carter;
using Course.API.Models;
using MediatR;

namespace Course.API.Features.Lessons.UpdateLesson
{
    public class UpdateLessonEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/lessons/{lessonId}", async (string courseId, string lessonId, UpdateLessonRequest req, ISender sender) =>
            {
                var cmd = new UpdateLessonCommand(courseId, lessonId, req);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
