using Carter;
using Exam.Application.Exams.Commands.UpdateExam;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace Exam.API.Endpoints
{
    public class UpdateExam : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/exams/{id}", async (string id, Dictionary<string, object> updates, ISender sender) =>
            {
                var cmd = new UpdateExamCommand(id, updates);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
