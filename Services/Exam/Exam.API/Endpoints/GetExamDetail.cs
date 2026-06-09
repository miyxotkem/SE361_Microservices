using Carter;
using Exam.Application.Exams.Queries.GetExamDetail;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class GetExamDetail : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/{id}", async (string id, ISender sender) =>
            {
                var query = new GetExamDetailQuery(id);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
