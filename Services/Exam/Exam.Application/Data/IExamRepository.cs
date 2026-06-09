using System.Collections.Generic;
using System.Threading.Tasks;
using Exam.Domain.Models;

namespace Exam.Application.Data
{
    public interface IExamRepository
    {
        Task<Exam.Domain.Models.Exam?> GetByIdAsync(string id);
        Task<List<Exam.Domain.Models.Exam>> GetExamsByCourseIdAsync(string courseId);
        Task<List<Exam.Domain.Models.Exam>> GetExamsByInstructorIdAsync(string instructorId);
        Task<string> CreateAsync(Exam.Domain.Models.Exam exam);
        Task UpdateAsync(string id, Dictionary<string, object> updates);
        Task DeleteAsync(string id);

        Task<ExamSubmission?> GetSubmissionByIdAsync(string submissionId);
        Task<List<ExamSubmission>> GetSubmissionsByExamIdAsync(string examId);
        Task<List<ExamSubmission>> GetSubmissionsByStudentIdAsync(string studentId);
        Task CreateSubmissionAsync(ExamSubmission submission);

        Task<ExamDraft?> GetDraftAsync(string examId, string studentId);
        Task SaveDraftAsync(ExamDraft draft);
        Task DeleteDraftAsync(string examId, string studentId);

        Task<bool> DeleteQuestionAsync(string examId, string questionId);
    }
}
