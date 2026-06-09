using BuildingBlocks.CQRS;
using Exam.Application.Data;
using Exam.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetMyExams
{
    public class GetMyExamsQueryHandler : IQueryHandler<GetMyExamsQuery, IResult>
    {
        private readonly IExamRepository _examRepository;
        private readonly ICourseServiceClient _courseServiceClient;

        public GetMyExamsQueryHandler(IExamRepository examRepository, ICourseServiceClient courseServiceClient)
        {
            _examRepository = examRepository;
            _courseServiceClient = courseServiceClient;
        }

        public async Task<IResult> Handle(GetMyExamsQuery request, CancellationToken cancellationToken)
        {
            // gRPC Call: Get student course registrations
            var courseIds = await _courseServiceClient.GetAcceptedCoursesForStudentAsync(request.StudentId);
            if (courseIds.Count == 0) return Results.Ok(new List<object>());

            var exams = new List<object>();
            foreach (var courseId in courseIds)
            {
                var courseExams = await _examRepository.GetExamsByCourseIdAsync(courseId);
                var activeExams = courseExams
                    .Where(e => e.IsPublished)
                    .Select(e => new { Id = e.Id, Data = ExamsHelper.CleanExamData(e) });
                exams.AddRange(activeExams);
            }

            return Results.Ok(exams);
        }
    }
}
