using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Courses.CreateCourse
{
    public record CreateCourseCommand(CreateCourseRequest Request) : ICommand<IResult>;

    public class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public CreateCourseCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            var courseData = new Dictionary<string, object>
            {
                { "Title", req.Title },
                { "Description", req.Description },
                { "ThumbnailUrl", req.ThumbnailUrl },
                { "Price", (double)req.Price },
                { "ClassName", req.ClassName },
                { "CourseType", req.CourseType },
                { "Category", req.Category },
                { "DayOfWeek", req.DayOfWeek },
                { "StartPeriod", req.StartPeriod },
                { "EndPeriod", req.EndPeriod },
                { "Semester", req.Semester },
                { "Emoji", req.Emoji },
                { "AccentColor", req.AccentColor },
                { "InstructorId", req.InstructorId },
                { "CreatedAt", DateTime.UtcNow },
                { "IsActive", req.IsActive },
                { "StudentCount", req.StudentCount },
                { "AssignmentCount", req.AssignmentCount }
            };

            DocumentReference docRef;
            if (!string.IsNullOrEmpty(req.Courseid))
            {
                docRef = _firestoreDb.Collection("Courses").Document(req.Courseid);
                await docRef.SetAsync(courseData, cancellationToken: cancellationToken);
            }
            else
            {
                docRef = await _firestoreDb.Collection("Courses").AddAsync(courseData, cancellationToken: cancellationToken);
            }

            return Results.Ok(new { Message = "Course created successfully.", Id = docRef.Id });
        }
    }
}
