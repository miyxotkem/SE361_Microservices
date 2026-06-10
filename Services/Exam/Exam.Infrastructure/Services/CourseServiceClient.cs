using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Course.API.Grpc;
using Exam.Application.Services;

namespace Exam.Infrastructure.Services
{
    public class CourseServiceClient : ICourseServiceClient
    {
        private readonly CourseProtoService.CourseProtoServiceClient _client;

        public CourseServiceClient(CourseProtoService.CourseProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<bool> IsStudentAcceptedInCourseAsync(string studentId, string courseId)
        {
            try
            {
                var response = await _client.CheckStudentRegistrationAsync(new CheckStudentRegistrationRequest
                {
                    StudentId = studentId,
                    CourseId = courseId
                });
                return response.IsAccepted;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAcceptedCoursesForStudentAsync(string studentId)
        {
            try
            {
                var response = await _client.GetAcceptedCoursesForStudentAsync(new GetAcceptedCoursesRequest
                {
                    StudentId = studentId
                });
                return response.CourseIds.ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<string> GetCourseClassNameAsync(string courseId)
        {
            try
            {
                var response = await _client.GetCourseDetailsAsync(new GetCourseDetailsRequest
                {
                    CourseId = courseId
                });
                
                if (string.IsNullOrEmpty(response.ClassName))
                {
                    return response.Title ?? string.Empty;
                }
                if (string.IsNullOrEmpty(response.Title))
                {
                    return response.ClassName ?? string.Empty;
                }
                return $"{response.ClassName} - {response.Title}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
