using Carter;
using Exam.Application.Exams.Commands.DeleteExamDraft;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class DeleteExamDraft : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/exams/{examId}/drafts/{studentId}", async (string examId, string studentId, ISender sender) =>
            {
                var cmd = new DeleteExamDraftCommand(examId, studentId);
                return await sender.Send(cmd);
            }).AllowAnonymous();
        }
    }
}
