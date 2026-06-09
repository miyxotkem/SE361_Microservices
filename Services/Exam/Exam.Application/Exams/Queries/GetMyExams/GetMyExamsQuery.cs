using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetMyExams
{
    public record GetMyExamsQuery(string StudentId) : IQuery<IResult>;
}
