using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Exam.Application.Exams.Commands.SaveExamDraft
{
    public class SaveExamDraftCommandHandler : ICommandHandler<SaveExamDraftCommand, IResult>
    {
        private readonly IExamRepository _examRepository;

        public SaveExamDraftCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(SaveExamDraftCommand request, CancellationToken cancellationToken)
        {
            var draft = request.Draft;
            draft.StartedAt = DateTime.SpecifyKind(draft.StartedAt, DateTimeKind.Utc);
            draft.SavedAt = DateTime.UtcNow;

            await _examRepository.SaveDraftAsync(draft);
            return Results.Ok(draft);
        }
    }
}
