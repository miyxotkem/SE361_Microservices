using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamDraft
{
    public record GetExamDraftQuery(string ExamId, string StudentId) : IQuery<IResult>;
}
