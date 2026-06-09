using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExam
{
    public record DeleteExamCommand(string Id) : ICommand<IResult>;
}
