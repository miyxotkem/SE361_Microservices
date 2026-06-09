namespace Exam.Domain.Models
{
    public class AnswerResponse
    {
        public string QuestionId { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public double PointsEarned { get; set; }

        public AnswerResponse() { }

        public AnswerResponse(string questionId, int questionOrder, string studentAnswer, bool isCorrect, double pointsEarned)
        {
            QuestionId = questionId;
            QuestionOrder = questionOrder;
            StudentAnswer = studentAnswer;
            IsCorrect = isCorrect;
            PointsEarned = pointsEarned;
        }
    }
}
