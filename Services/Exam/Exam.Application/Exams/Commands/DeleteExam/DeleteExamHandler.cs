using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExam
{
    public class DeleteExamCommandHandler : ICommandHandler<DeleteExamCommand, IResult>
    {
        private readonly IExamRepository _examRepository;

        public DeleteExamCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {
            await _examRepository.DeleteAsync(request.Id);
            return Results.Ok(new { Message = "Exam deleted successfully." });
        }
    }
}
