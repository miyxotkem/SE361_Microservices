using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Lessons.DeleteLesson
{
    public record DeleteLessonCommand(string CourseId, string LessonId) : ICommand<IResult>;

    public class DeleteLessonCommandHandler : ICommandHandler<DeleteLessonCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteLessonCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            var lessonRef = _firestoreDb.Collection("Lessons").Document(request.LessonId);
            var lessonSnap = await lessonRef.GetSnapshotAsync(cancellationToken);
            if (!lessonSnap.Exists || lessonSnap.GetValue<string>("CourseId") != request.CourseId) return Results.NotFound();

            await lessonRef.DeleteAsync(cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Lesson deleted successfully." });
        }
    }
}
