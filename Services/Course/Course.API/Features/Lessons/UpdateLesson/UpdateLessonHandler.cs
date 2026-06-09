using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Lessons.UpdateLesson
{
    public record UpdateLessonCommand(string CourseId, string LessonId, UpdateLessonRequest Request) : ICommand<IResult>;

    public class UpdateLessonCommandHandler : ICommandHandler<UpdateLessonCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateLessonCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            var lessonRef = _firestoreDb.Collection("Lessons").Document(request.LessonId);
            var lessonSnap = await lessonRef.GetSnapshotAsync(cancellationToken);
            if (!lessonSnap.Exists || lessonSnap.GetValue<string>("CourseId") != request.CourseId) return Results.NotFound("Lesson not found.");

            var updates = new Dictionary<string, object>
            {
                { "Title", request.Request.Title },
                { "VideoUrl", request.Request.VideoUrl },
                { "DocumentUrl", request.Request.DocumentUrl },
                { "Description", request.Request.Description },
                { "Order", request.Request.Order },
                { "UpdatedAt", DateTime.UtcNow }
            };

            await lessonRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Lesson updated successfully." });
        }
    }
}
