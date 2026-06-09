using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Queries.GetExamDraft
{
    public class GetExamDraftQueryHandler : IQueryHandler<GetExamDraftQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetExamDraftQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetExamDraftQuery request, CancellationToken cancellationToken)
        {
            var draft = await _examRepository.GetDraftAsync(request.ExamId, request.StudentId);
            return Results.Ok(draft);
        }
    }
}
