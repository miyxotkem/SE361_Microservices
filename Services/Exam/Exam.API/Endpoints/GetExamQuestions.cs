using Carter;
using Exam.Application.Exams.Queries.GetExamQuestions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class GetExamQuestions : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/{id}/questions", async (string id, ISender sender) =>
            {
                var query = new GetExamQuestionsQuery(id);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
