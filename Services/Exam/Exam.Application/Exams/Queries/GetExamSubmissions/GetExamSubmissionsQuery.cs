using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamSubmissions
{
    public record GetExamSubmissionsQuery(string ExamId) : IQuery<IResult>;
}
