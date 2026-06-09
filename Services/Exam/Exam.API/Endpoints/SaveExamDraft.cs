using Carter;
using Exam.Application.Exams.Commands.SaveExamDraft;
using Exam.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class SaveExamDraft : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/exams/drafts", async (ExamDraft draft, ISender sender) =>
            {
                var cmd = new SaveExamDraftCommand(draft);
                return await sender.Send(cmd);
            }).AllowAnonymous();
        }
    }
}
