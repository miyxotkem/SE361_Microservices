using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.DeleteExamQuestion
{
    public class DeleteExamQuestionCommandHandler : ICommandHandler<DeleteExamQuestionCommand, IResult>
    {
        private readonly IExamRepository _examRepository;

        public DeleteExamQuestionCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(DeleteExamQuestionCommand request, CancellationToken cancellationToken)
        {
            bool success = await _examRepository.DeleteQuestionAsync(request.Id, request.QuestionId);
            if (!success)
            {
                return Results.NotFound(new { Message = "Question or Exam not found." });
            }

            return Results.Ok(new { success = true, Message = "Question deleted successfully." });
        }
    }
}
