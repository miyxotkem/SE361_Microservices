using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Application.Services
{
    public interface ICourseServiceClient
    {
        Task<bool> IsStudentAcceptedInCourseAsync(string studentId, string courseId);
        Task<List<string>> GetAcceptedCoursesForStudentAsync(string studentId);
        Task<string> GetCourseClassNameAsync(string courseId);
    }
}
