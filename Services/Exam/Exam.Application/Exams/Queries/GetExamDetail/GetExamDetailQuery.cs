using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamDetail
{
    public record GetExamDetailQuery(string Id) : IQuery<IResult>;
}
