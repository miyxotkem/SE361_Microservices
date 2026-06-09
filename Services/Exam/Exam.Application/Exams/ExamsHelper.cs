using System.Collections.Generic;

namespace Exam.Application.Exams
{
    public static class ExamsHelper
    {
        public static Dictionary<string, object> CleanExamData(Exam.Domain.Models.Exam exam)
        {
            // Sync properties time limit and duration for backward compatibility
            int duration = exam.DurationMinutes > 0 ? exam.DurationMinutes : exam.TimeLimitMinutes;
            return new Dictionary<string, object>
            {
                { "Id", exam.Id },
                { "ClassId", exam.ClassId },
                { "ClassName", exam.ClassName },
                { "Title", exam.Title },
                { "DurationMinutes", duration },
                { "TimeLimitMinutes", duration },
                { "Description", exam.Description },
                { "PassingScore", exam.PassingScore },
                { "IsPublished", exam.IsPublished },
                { "IsActive", exam.IsActive },
                { "AllowReview", exam.AllowReview },
                { "RandomizeQuestions", exam.RandomizeQuestions },
                { "ShowScore", exam.ShowScore },
                { "AllowMultipleAttempts", exam.AllowMultipleAttempts },
                { "MaxAttempts", exam.MaxAttempts },
                { "TotalQuestions", exam.TotalQuestions },
                { "CreatedAt", exam.CreatedAt },
                { "UpdatedAt", exam.UpdatedAt },
                { "InstructorId", exam.InstructorId }
            };
        }
    }
}
