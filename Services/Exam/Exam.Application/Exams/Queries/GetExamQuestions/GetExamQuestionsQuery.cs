using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamQuestions
{
    public record GetExamQuestionsQuery(string Id) : IQuery<IResult>;
}
