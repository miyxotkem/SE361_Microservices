using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class Question
    {
        public string QuestionId { get; private set; } = string.Empty;
        public string QuestionText { get; private set; } = string.Empty;
        public List<string> Options { get; private set; } = new();
        public int CorrectOptionIndex { get; private set; }
        public double Points { get; private set; }

        private Question() { } // For deserialization

        public Question(string questionId, string questionText, List<string> options, int correctOptionIndex, double points)
        {
            if (string.IsNullOrWhiteSpace(questionText))
                throw new ArgumentException("Question text cannot be empty.", nameof(questionText));
            if (options == null || options.Count < 2)
                throw new ArgumentException("Question must have at least 2 options.", nameof(options));
            if (correctOptionIndex < 0 || correctOptionIndex >= options.Count)
                throw new ArgumentOutOfRangeException(nameof(correctOptionIndex), "Correct option index is out of bounds.");
            if (points < 0)
                throw new ArgumentOutOfRangeException(nameof(points), "Points cannot be negative.");

            QuestionId = questionId;
            QuestionText = questionText;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
            Points = points;
        }
    }
}
