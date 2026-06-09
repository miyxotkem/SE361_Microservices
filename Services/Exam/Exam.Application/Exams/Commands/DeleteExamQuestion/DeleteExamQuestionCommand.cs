using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExamQuestion
{
    public record DeleteExamQuestionCommand(string Id, string QuestionId) : ICommand<IResult>;
}
