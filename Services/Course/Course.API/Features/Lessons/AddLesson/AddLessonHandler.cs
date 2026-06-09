using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Lessons.AddLesson
{
    public record AddLessonCommand(string CourseId, CreateLessonRequest Request) : ICommand<IResult>;

    public class AddLessonCommandHandler : ICommandHandler<AddLessonCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public AddLessonCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(AddLessonCommand request, CancellationToken cancellationToken)
        {
            var courseRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            if (!(await courseRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Course not found.");

            var lessonData = new Dictionary<string, object>
            {
                { "CourseId", request.CourseId },
                { "Title", request.Request.Title },
                { "VideoUrl", request.Request.VideoUrl },
                { "DocumentUrl", request.Request.DocumentUrl },
                { "Description", request.Request.Description },
                { "Order", request.Request.Order },
                { "CreatedAt", DateTime.UtcNow }
            };

            var lessonRef = await _firestoreDb.Collection("Lessons").AddAsync(lessonData, cancellationToken: cancellationToken);

            var lessonResponse = new
            {
                Id = lessonRef.Id,
                Data = new
                {
                    Id = lessonRef.Id,
                    CourseId = request.CourseId,
                    Title = request.Request.Title,
                    VideoUrl = request.Request.VideoUrl,
                    DocumentUrl = request.Request.DocumentUrl,
                    Description = request.Request.Description,
                    CreatedAt = DateTime.UtcNow
                }
            };
            return Results.Ok(lessonResponse);
        }
    }
}
