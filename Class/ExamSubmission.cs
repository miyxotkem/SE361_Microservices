using System;
using System.Collections.Generic;

namespace e_learning_app.Class
{
    /// <summary>
    /// Kết quả làm bài của học sinh
    /// </summary>
    public class ExamSubmission
    {
        public string Id { get; set; }                    // Unique ID
        public string ExamId { get; set; }                // Bài thi
        public string StudentId { get; set; }             // Học sinh
        public string StudentName { get; set; }           // Tên học sinh

        // Thời gian
        public DateTime SubmittedAt { get; set; }         // Nộp bài lúc
        public int TimeSpentSeconds { get; set; }         // Thời gian làm bài (giây)

        // Kết quả
        public List<AnswerResponse> Answers { get; set; }  // Các câu trả lời
        public double Score { get; set; }                  // Điểm số
        public double Percentage { get; set; }             // Phần trăm (%)
        public SubmissionStatus Status { get; set; }       // Trạng thái

        // Nhận xét
        public string FeedbackFromTeacher { get; set; }    // Nhận xét giáo viên
        public DateTime? GradedAt { get; set; }            // Lúc chấm điểm

        public ExamSubmission()
        {
            Id = Guid.NewGuid().ToString();
            Answers = new List<AnswerResponse>();
            SubmittedAt = DateTime.Now;
            Status = SubmissionStatus.Submitted;
        }
    }

    /// <summary>
    /// Câu trả lời của học sinh
    /// </summary>
    public class AnswerResponse
    {
        public string QuestionId { get; set; }
        public int QuestionOrder { get; set; }
        public string StudentAnswer { get; set; }         // Câu trả lời (text hoặc index)
        public bool? IsCorrect { get; set; }              // Đúng/Sai (null nếu chưa chấm)
        public double PointsEarned { get; set; }          // Điểm đạt được
    }

    /// <summary>
    /// Trạng thái nộp bài
    /// </summary>
    public enum SubmissionStatus
    {
        InProgress,         // Đang làm
        Submitted,          // Đã nộp
        Graded,             // Đã chấm
        Expired             // Hết hạn
    }
}