using Carter;
using Exam.Application.Exams.Queries.GetExamSubmissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class GetExamSubmissions : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/{id}/submissions", async (string id, ISender sender) =>
            {
                var query = new GetExamSubmissionsQuery(id);
                return await sender.Send(query);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
