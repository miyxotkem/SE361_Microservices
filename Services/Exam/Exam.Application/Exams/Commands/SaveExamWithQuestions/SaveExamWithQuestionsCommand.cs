using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Exam.Application.Exams.Commands.SaveExamWithQuestions
{
    public record SaveExamWithQuestionsCommand(
        string ExamId,
        string ClassId,
        string? ClassName,
        string? Title,
        string? Description,
        int TimeLimitMinutes,
        double PassingScore,
        bool IsPublished,
        bool IsActive,
        bool AllowReview,
        bool RandomizeQuestions,
        bool ShowScore,
        bool AllowMultipleAttempts,
        int MaxAttempts,
        List<SaveExamQuestionDto> Questions,
        string CurrentUserId
    ) : ICommand<IResult>;

    public class SaveExamQuestionDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public double Points { get; set; } = 1.0;
    }
}
