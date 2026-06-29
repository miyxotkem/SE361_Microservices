using System;
using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamsForCourse
{
    public record GetExamsForCourseQuery(string CourseId) : ICachedQuery<IResult>
    {
        public string CacheKey => $"GetExamsForCourse-{CourseId}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
