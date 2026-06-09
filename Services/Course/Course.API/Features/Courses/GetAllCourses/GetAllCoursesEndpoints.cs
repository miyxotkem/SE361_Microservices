using Carter;
using MediatR;

namespace Course.API.Features.Courses.GetAllCourses
{
    public class GetAllCoursesEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses", async (ISender sender) =>
            {
                var query = new GetAllCoursesQuery();
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
