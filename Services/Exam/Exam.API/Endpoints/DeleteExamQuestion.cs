using Carter;
using Exam.Application.Exams.Commands.DeleteExamQuestion;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Exam.API.Endpoints
{
    public class DeleteExamQuestion : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/exams/{id}/questions/{questionId}", async (string id, string questionId, ISender sender) =>
            {
                var command = new DeleteExamQuestionCommand(id, questionId);
                return await sender.Send(command);
            }).RequireAuthorization();
        }
    }
}
