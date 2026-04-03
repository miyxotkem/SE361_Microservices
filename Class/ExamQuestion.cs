using System;
using System.Collections.Generic;

namespace e_learning_app.Class
{
    /// <summary>
    /// Câu hỏi trong bài thi (với cấu hình riêng cho bài thi)
    /// </summary>
    public class ExamQuestion
    {
        public string Id { get; set; }                    // ID câu hỏi
        public int QuestionOrder { get; set; }            // Thứ tự câu hỏi
        public QuestionType Type { get; set; }            // Loại câu hỏi
        public string Content { get; set; }               // Nội dung câu hỏi

        // Cho câu trắc nghiệm
        public List<string> Options { get; set; }         // Các đáp án
        public int CorrectAnswerIndex { get; set; }       // Đáp án đúng (index)

        // Cho câu tự luận
        public int MaxWords { get; set; }                 // Tối đa số từ

        // Điểm
        public double Points { get; set; }                // Điểm của câu (mặc định = 1)

        public ExamQuestion()
        {
            Id = Guid.NewGuid().ToString();
            Options = new List<string>();
            Points = 1;
        }
    }

    /// <summary>
    /// Loại câu hỏi
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice,     // Trắc nghiệm
        TrueFalse,         // Đúng/Sai
        ShortAnswer,       // Câu trả lời ngắn
        Essay,             // Tự luận
        Matching           // Ghép đôi
    }
}