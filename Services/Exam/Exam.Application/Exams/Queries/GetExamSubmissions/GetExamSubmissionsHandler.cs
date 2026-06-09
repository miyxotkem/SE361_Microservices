using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetExamSubmissions
{
    public class GetExamSubmissionsQueryHandler : IQueryHandler<GetExamSubmissionsQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetExamSubmissionsQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetExamSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var subs = await _examRepository.GetSubmissionsByExamIdAsync(request.ExamId);
            var results = subs.Select(s => new { Id = s.Id, Data = s }).ToList();
            return Results.Ok(results);
        }
    }
}
