using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamsForCourse
{
    public record GetExamsForCourseQuery(string CourseId) : IQuery<IResult>;
}
