using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Contents.DeleteCourseContent
{
    public record DeleteCourseContentCommand(string CourseId, string ContentId) : ICommand<IResult>;

    public class DeleteCourseContentCommandHandler : ICommandHandler<DeleteCourseContentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteCourseContentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteCourseContentCommand request, CancellationToken cancellationToken)
        {
            var contentRef = _firestoreDb.Collection("Courses").Document(request.CourseId).Collection("Contents").Document(request.ContentId);
            if (!(await contentRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Content not found.");

            await contentRef.DeleteAsync(cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Content deleted successfully." });
        }
    }
}
