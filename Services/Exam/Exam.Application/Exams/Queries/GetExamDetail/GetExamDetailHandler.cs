using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQueryHandler : IQueryHandler<GetExamDetailQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetExamDetailQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
        {
            var exam = await _examRepository.GetByIdAsync(request.Id);
            if (exam == null) return Results.NotFound(new { Message = "Exam not found." });

            var cleaned = ExamsHelper.CleanExamData(exam);
            
            // Format Questions list for client
            var questionsList = exam.Questions.Select(q => new Dictionary<string, object>
            {
                { "QuestionId", q.QuestionId },
                { "QuestionText", q.QuestionText },
                { "Options", q.Options },
                { "CorrectOptionIndex", q.CorrectOptionIndex },
                { "Points", q.Points }
            }).ToList();

            cleaned["Questions"] = questionsList;
            cleaned["QuestionIds"] = exam.QuestionIds;

            return Results.Ok(new { Id = exam.Id, Data = cleaned });
        }
    }
}
