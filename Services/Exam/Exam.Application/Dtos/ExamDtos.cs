using System.Collections.Generic;

namespace Exam.Application.Dtos
{
    public class CreateExamRequest
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
        public double PassingScore { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
        public bool AllowReview { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool ShowScore { get; set; }
        public bool AllowMultipleAttempts { get; set; }
        public int MaxAttempts { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class QuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public double Points { get; set; }
    }

    public class SubmitExamRequest
    {
        public Dictionary<string, int> Answers { get; set; } = new();
        public int TimeSpentSeconds { get; set; }
    }
}
