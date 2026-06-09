using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetMyHistory
{
    public record GetMyHistoryQuery(string StudentId) : IQuery<IResult>;
}
