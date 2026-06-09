using Carter;
using MediatR;

namespace Course.API.Features.Courses.DeleteCourse
{
    public class DeleteCourseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{id}", async (string id, ISender sender) =>
            {
                var cmd = new DeleteCourseCommand(id);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
