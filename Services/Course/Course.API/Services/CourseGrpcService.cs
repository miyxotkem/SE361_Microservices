using Google.Cloud.Firestore;
using Grpc.Core;
using Course.API.Grpc;

namespace Course.API.Services
{
    public class CourseGrpcService : CourseProtoService.CourseProtoServiceBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<CourseGrpcService> _logger;

        public CourseGrpcService(FirestoreDb firestoreDb, ILogger<CourseGrpcService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public override async Task<CourseModel> GetCourseDetails(GetCourseDetailsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetCourseDetails called for CourseId: {CourseId}", request.CourseId);

            var docRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            var snap = await docRef.GetSnapshotAsync(context.CancellationToken);

            if (!snap.Exists)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Course with ID {request.CourseId} not found."));
            }

            var dict = snap.ToDictionary();
            string title = dict.ContainsKey("Title") ? dict["Title"].ToString() ?? "" : "";
            string className = dict.ContainsKey("ClassName") ? dict["ClassName"].ToString() ?? "" : "";
            string instructorId = dict.ContainsKey("InstructorId") ? dict["InstructorId"].ToString() ?? "" : "";

            return new CourseModel
            {
                CourseId = request.CourseId,
                Title = title,
                ClassName = className,
                InstructorId = instructorId
            };
        }

        public override async Task<StudentRegistrationModel> CheckStudentRegistration(CheckStudentRegistrationRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC CheckStudentRegistration called for StudentId: {StudentId}, CourseId: {CourseId}", request.StudentId, request.CourseId);

            // Document ID is formatted as {studentId}_{courseId}
            string regDocId = $"{request.StudentId}_{request.CourseId}";
            var docRef = _firestoreDb.Collection("courseRegistrations").Document(regDocId);
            var snap = await docRef.GetSnapshotAsync(context.CancellationToken);

            if (!snap.Exists)
            {
                return new StudentRegistrationModel
                {
                    IsAccepted = false,
                    CourseId = request.CourseId,
                    ClassName = ""
                };
            }

            var dict = snap.ToDictionary();
            string status = dict.ContainsKey("status") ? dict["status"].ToString() ?? "" : "";
            bool isAccepted = status.Equals("accepted", StringComparison.OrdinalIgnoreCase) || status.Equals("active", StringComparison.OrdinalIgnoreCase);

            string className = "";
            if (isAccepted)
            {
                var courseSnap = await _firestoreDb.Collection("Courses").Document(request.CourseId).GetSnapshotAsync(context.CancellationToken);
                if (courseSnap.Exists)
                {
                    var courseDict = courseSnap.ToDictionary();
                    className = courseDict.ContainsKey("ClassName") ? courseDict["ClassName"].ToString() ?? "" : "";
                }
            }

            return new StudentRegistrationModel
            {
                IsAccepted = isAccepted,
                CourseId = request.CourseId,
                ClassName = className
            };
        }

        public override async Task<AcceptedCoursesResponse> GetAcceptedCoursesForStudent(GetAcceptedCoursesRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetAcceptedCoursesForStudent called for StudentId: {StudentId}", request.StudentId);

            var snap = await _firestoreDb.Collection("courseRegistrations")
                .WhereEqualTo("userId", request.StudentId)
                .GetSnapshotAsync(context.CancellationToken);

            var acceptedCourseIds = new List<string>();

            foreach (var doc in snap.Documents)
            {
                var dict = doc.ToDictionary();
                string status = dict.ContainsKey("status") ? dict["status"].ToString() ?? "" : "";
                if (status.Equals("accepted", StringComparison.OrdinalIgnoreCase) || status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    string courseId = dict.ContainsKey("courseId") ? dict["courseId"].ToString() ?? "" : "";
                    if (!string.IsNullOrEmpty(courseId))
                    {
                        acceptedCourseIds.Add(courseId);
                    }
                }
            }

            var response = new AcceptedCoursesResponse();
            response.CourseIds.AddRange(acceptedCourseIds);
            return response;
        }
    }
}
