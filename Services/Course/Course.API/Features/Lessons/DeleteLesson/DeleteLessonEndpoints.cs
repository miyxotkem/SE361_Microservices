using Carter;
using MediatR;

namespace Course.API.Features.Lessons.DeleteLesson
{
    public class DeleteLessonEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{courseId}/lessons/{lessonId}", async (string courseId, string lessonId, ISender sender) =>
            {
                var cmd = new DeleteLessonCommand(courseId, lessonId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
