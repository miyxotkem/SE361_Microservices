using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Registrations.RegisterCourse
{
    public record RegisterCourseCommand(string CourseId, string Uid) : ICommand<IResult>;

    public class RegisterCourseCommandHandler : ICommandHandler<RegisterCourseCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public RegisterCourseCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(RegisterCourseCommand request, CancellationToken cancellationToken)
        {
            var courseRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            if (!(await courseRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Course not found.");

            string regId = $"{request.Uid}_{request.CourseId}";
            var regData = new Dictionary<string, object>
            {
                { "userId", request.Uid },
                { "courseId", request.CourseId },
                { "requestDate", FieldValue.ServerTimestamp },
                { "status", "pending" },
                { "approvedDate", null! },
                { "progressPercentage", 0.0 }
            };

            await _firestoreDb.Collection("courseRegistrations").Document(regId).SetAsync(regData, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Successfully registered for the course." });
        }
    }
}
