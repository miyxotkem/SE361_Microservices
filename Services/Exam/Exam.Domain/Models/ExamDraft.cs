using System;
using System.Collections.Generic;

namespace Exam.Domain.Models
{
    public class ExamDraft
    {
        public string? Id { get; set; } = string.Empty;
        public string ExamId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public Dictionary<string, string> Answers { get; set; } = new();
        public List<string> MarkedForReview { get; set; } = new();
        public int LastQuestionIndex { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
