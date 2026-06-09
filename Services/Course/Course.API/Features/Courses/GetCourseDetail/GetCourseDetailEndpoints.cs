using Carter;
using MediatR;

namespace Course.API.Features.Courses.GetCourseDetail
{
    public class GetCourseDetailEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/{id}", async (string id, ISender sender) =>
            {
                var query = new GetCourseDetailQuery(id);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
