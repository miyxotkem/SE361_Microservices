using Carter;
using MediatR;
using System.Text.Json;

namespace Course.API.Features.Courses.UpdateCourse
{
    public class UpdateCourseEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{id}", async (string id, JsonElement requestBody, ISender sender) =>
            {
                var cmd = new UpdateCourseCommand(id, requestBody);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
