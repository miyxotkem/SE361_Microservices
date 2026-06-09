using Carter;
using Exam.Application.Dtos;
using Exam.Application.Exams.Commands.SubmitExam;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Exam.API.Endpoints
{
    public class SubmitExam : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/exams/{id}/submit", async (string id, SubmitExamRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new SubmitExamCommand(id, uid, req.Answers, req.TimeSpentSeconds);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
