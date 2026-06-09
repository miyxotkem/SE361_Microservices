using Carter;
using MediatR;

namespace Course.API.Features.Registrations.GetCourseStudents
{
    public class GetCourseStudentsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{courseId}/students", async (string courseId, ISender sender) =>
            {
                var query = new GetCourseStudentsQuery(courseId);
                return await sender.Send(query);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
