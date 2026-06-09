using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetAllExams
{
    public record GetAllExamsQuery(string InstructorId) : IQuery<IResult>;
}
