using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Exam.Application.Exams.Commands.UpdateExam
{
    public record UpdateExamCommand(string Id, Dictionary<string, object> Updates) : ICommand<IResult>;
}
