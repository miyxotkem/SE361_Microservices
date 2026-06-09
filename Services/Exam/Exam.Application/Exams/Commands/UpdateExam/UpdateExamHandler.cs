using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.UpdateExam
{
    public class UpdateExamCommandHandler : ICommandHandler<UpdateExamCommand, IResult>
    {
        private readonly IExamRepository _examRepository;

        public UpdateExamCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
        {
            await _examRepository.UpdateAsync(request.Id, request.Updates);
            return Results.Ok(new { Message = "Exam updated successfully." });
        }
    }
}
