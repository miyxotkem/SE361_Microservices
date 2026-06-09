using BuildingBlocks.CQRS;
using Exam.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.SaveExamDraft
{
    public record SaveExamDraftCommand(ExamDraft Draft) : ICommand<IResult>;
}
