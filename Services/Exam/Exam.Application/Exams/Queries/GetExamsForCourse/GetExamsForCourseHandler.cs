using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetExamsForCourse
{
    public class GetExamsForCourseQueryHandler : IQueryHandler<GetExamsForCourseQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetExamsForCourseQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetExamsForCourseQuery request, CancellationToken cancellationToken)
        {
            var exams = await _examRepository.GetExamsByCourseIdAsync(request.CourseId);
            var results = exams.Select(e => new { Id = e.Id, Data = ExamsHelper.CleanExamData(e) }).ToList();
            return Results.Ok(results);
        }
    }
}
