using Carter;
using Exam.Application.Exams.Queries.GetExamsForCourse;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class GetExamsForCourse : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/course/{courseId}", async (string courseId, ISender sender) =>
            {
                var query = new GetExamsForCourseQuery(courseId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
