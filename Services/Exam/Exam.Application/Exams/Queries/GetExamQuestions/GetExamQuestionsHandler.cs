using BuildingBlocks.CQRS;
using Exam.Application.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Exam.Application.Exams.Queries.GetExamQuestions
{
    public class GetExamQuestionsQueryHandler : IQueryHandler<GetExamQuestionsQuery, IResult>
    {
        private readonly IExamRepository _examRepository;

        public GetExamQuestionsQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IResult> Handle(GetExamQuestionsQuery request, CancellationToken cancellationToken)
        {
            var exam = await _examRepository.GetByIdAsync(request.Id);
            if (exam == null) return Results.NotFound(new { Message = "Exam not found." });

            var questionsList = new List<Dictionary<string, object>>();
            int order = 1;
            foreach (var q in exam.Questions)
            {
                var qDict = new Dictionary<string, object>
                {
                    { "Id", q.QuestionId },
                    { "QuestionOrder", order++ },
                    { "Type", "MultipleChoice" },
                    { "Content", q.QuestionText },
                    { "QuestionText", q.QuestionText },
                    { "Options", q.Options },
                    { "CorrectAnswerIndex", q.CorrectOptionIndex },
                    { "CorrectOptionIndex", q.CorrectOptionIndex },
                    { "Points", q.Points },
                    { "MaxWords", 0 }
                };
                questionsList.Add(qDict);
            }

            return Results.Ok(questionsList);
        }
    }
}
