using System;
using System.Collections.Generic;
using Exam.Domain.Entities;

namespace Exam.Domain.Models
{
    public class Exam
    {
        public string Id { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TimeLimitMinutes { get; set; } // backward compatibility
        public string Description { get; set; } = string.Empty;
        public double PassingScore { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
        public bool AllowReview { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool ShowScore { get; set; }
        public bool AllowMultipleAttempts { get; set; }
        public int MaxAttempts { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string InstructorId { get; set; } = string.Empty;

        // Domain Child Entities
        public List<Question> Questions { get; set; } = new();
        public List<string> QuestionIds { get; set; } = new();

        public Exam() { }

        public void AddQuestion(Question question)
        {
            if (IsPublished)
                throw new InvalidOperationException("Cannot add question to a published exam.");

            Questions.Add(question);
            QuestionIds.Add(question.QuestionId);
            TotalQuestions = Questions.Count;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Publish()
        {
            if (Questions.Count == 0)
                throw new InvalidOperationException("Cannot publish an exam without questions.");

            IsPublished = true;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Unpublish()
        {
            IsPublished = false;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
