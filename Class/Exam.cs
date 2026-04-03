using System;
using System.Collections.Generic;

namespace e_learning_app.Class
{
    /// <summary>
    /// Mô hình bài thi chính
    /// </summary>
    public class Exam
    {
        public string Id { get; set; }                    // Unique ID (Firebase)
        public string ClassId { get; set; }              // Lớp học này thuộc
        public string Title { get; set; }                // Tên bài thi
        public string Description { get; set; }          // Mô tả chi tiết
        public string SubjectCode { get; set; }          // Mã môn học

        // Cấu hình bài thi
        public int TotalQuestions { get; set; }          // Tổng số câu
        public int TimeLimitMinutes { get; set; }        // Giới hạn thời gian (phút)
        public double PassingScore { get; set; }         // Điểm qua (%)
        public ExamType Type { get; set; }               // Loại: Quiz, Midterm, Final, etc.

        // Các câu hỏi
        public List<string> QuestionIds { get; set; }    // Danh sách ID câu hỏi

        // Trạng thái
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ScheduledDate { get; set; }     // Thời gian thi
        public bool IsPublished { get; set; }            // Đã công bố?
        public bool IsActive { get; set; }               // Đang mở?

        // Cài đặt nâng cao
        public bool AllowReview { get; set; }            // Cho phép xem lại?
        public bool RandomizeQuestions { get; set; }     // Xáo trộn câu hỏi?
        public bool ShowScore { get; set; }              // Hiển thị điểm ngay?
        public bool AllowMultipleAttempts { get; set; }  // Cho phép thi nhiều lần?
        public int MaxAttempts { get; set; }             // Số lần thi tối đa

        public Exam()
        {
            Id = Guid.NewGuid().ToString();
            QuestionIds = new List<string>();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            TotalQuestions = 0;
            TimeLimitMinutes = 60;
            PassingScore = 50;
            Type = ExamType.Quiz;
            AllowReview = true;
            RandomizeQuestions = false;
            ShowScore = true;
            AllowMultipleAttempts = true;
            MaxAttempts = 3;
        }
    }

    /// <summary>
    /// Loại bài thi
    /// </summary>
    public enum ExamType
    {
        Quiz,           // Kiểm tra nhanh
        Midterm,        // Giữa kỳ
        Final,          // Cuối kỳ
        Practice,       // Luyện tập
        Assignment      // Bài tập
    }
}