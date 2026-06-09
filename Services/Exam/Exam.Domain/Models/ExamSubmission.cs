using System;
using System.Collections.Generic;

namespace Exam.Domain.Models
{
    public class ExamSubmission
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ExamId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public double Score { get; set; } // Out of 10.0 scale
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public List<AnswerResponse> Answers { get; set; } = new();
        public DateTime SubmittedAt { get; set; }
        public int TimeSpentSeconds { get; set; }

        public ExamSubmission() { }

        public void Grade(Exam exam, Dictionary<string, int> studentAnswers, string studentName)
        {
            if (exam == null) throw new ArgumentNullException(nameof(exam));
            
            StudentName = studentName;
            ExamId = exam.Id;
            CourseId = exam.ClassId;

            double earnedPoints = 0;
            double totalPoints = 0;
            int correctCount = 0;
            int totalQuestionsCount = exam.Questions.Count;
            var richAnswers = new List<AnswerResponse>();

            for (int i = 0; i < exam.Questions.Count; i++)
            {
                var question = exam.Questions[i];
                string qId = i.ToString(); // Client WPF expects index as QuestionId
                double questionPoints = question.Points;
                totalPoints += questionPoints;

                bool isCorrect = false;
                double pointsEarned = 0.0;
                string? studentAnswer = null;

                if (studentAnswers.TryGetValue(i.ToString(), out int studentOpt))
                {
                    studentAnswer = studentOpt.ToString();
                    if (question.CorrectOptionIndex == studentOpt)
                    {
                        correctCount++;
                        earnedPoints += questionPoints;
                        isCorrect = true;
                        pointsEarned = questionPoints;
                    }
                }

                richAnswers.Add(new AnswerResponse(qId, i + 1, studentAnswer ?? "", isCorrect, pointsEarned));
            }

            if (totalPoints == 0) totalPoints = totalQuestionsCount > 0 ? totalQuestionsCount : 1.0;
            double percentage = Math.Round((earnedPoints / totalPoints) * 100, 2);
            double finalScore = Math.Round(percentage / 10.0, 2);

            Answers = richAnswers;
            Score = finalScore;
            Percentage = percentage;
            TotalQuestions = totalQuestionsCount;
            SubmittedAt = DateTime.UtcNow;
        }
    }
}
