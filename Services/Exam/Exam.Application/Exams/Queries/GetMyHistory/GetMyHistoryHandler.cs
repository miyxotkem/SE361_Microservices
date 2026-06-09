using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetMyHistory
{
    public class GetMyHistoryQueryHandler : IQueryHandler<GetMyHistoryQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetMyHistoryQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetMyHistoryQuery request, CancellationToken cancellationToken)
        {
            var subs = await _examRepository.GetSubmissionsByStudentIdAsync(request.StudentId);
            var results = subs.Select(s => new { Id = s.Id, Data = s }).ToList();
            return Results.Ok(results);
        }
    }
}
