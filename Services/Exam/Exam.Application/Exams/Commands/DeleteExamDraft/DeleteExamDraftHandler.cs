using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExamDraft
{
    public class DeleteExamDraftCommandHandler : ICommandHandler<DeleteExamDraftCommand, IResult>
    {
        private readonly IExamRepository _examRepository;

        public DeleteExamDraftCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(DeleteExamDraftCommand request, CancellationToken cancellationToken)
        {
            await _examRepository.DeleteDraftAsync(request.ExamId, request.StudentId);
            return Results.Ok(new { success = true });
        }
    }
}
