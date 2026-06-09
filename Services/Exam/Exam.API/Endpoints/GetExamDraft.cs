using Carter;
using Exam.Application.Exams.Queries.GetExamDraft;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class GetExamDraft : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/{examId}/drafts/{studentId}", async (string examId, string studentId, ISender sender) =>
            {
                var query = new GetExamDraftQuery(examId, studentId);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
