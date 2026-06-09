using Carter;
using MediatR;

namespace Course.API.Features.Lessons.GetLessons
{
    public class GetLessonsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/lessons", async (string courseId, ISender sender) =>
            {
                var query = new GetLessonsQuery(courseId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
