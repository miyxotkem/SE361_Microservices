using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetAllExams
{
    public class GetAllExamsQueryHandler : IQueryHandler<GetAllExamsQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetAllExamsQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetAllExamsQuery request, CancellationToken cancellationToken)
        {
            var exams = await _examRepository.GetExamsByInstructorIdAsync(request.InstructorId);
            var results = exams.Select(e => new { Id = e.Id, Data = ExamsHelper.CleanExamData(e) }).ToList();
            return Results.Ok(results);
        }
    }
}
