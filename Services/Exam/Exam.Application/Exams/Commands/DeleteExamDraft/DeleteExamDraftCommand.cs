using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExamDraft
{
    public record DeleteExamDraftCommand(string ExamId, string StudentId) : ICommand<IResult>;
}
