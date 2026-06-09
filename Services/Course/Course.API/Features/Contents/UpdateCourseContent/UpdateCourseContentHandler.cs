using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Contents.UpdateCourseContent
{
    public record UpdateCourseContentCommand(string CourseId, string ContentId, UpdateCourseContentRequest Request) : ICommand<IResult>;

    public class UpdateCourseContentCommandHandler : ICommandHandler<UpdateCourseContentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateCourseContentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateCourseContentCommand request, CancellationToken cancellationToken)
        {
            var contentRef = _firestoreDb.Collection("Courses").Document(request.CourseId).Collection("Contents").Document(request.ContentId);
            if (!(await contentRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Content not found.");

            var updates = new Dictionary<string, object>();
            if (request.Request.Title != null) updates.Add("Title", request.Request.Title);
            if (request.Request.Type != null) updates.Add("Type", request.Request.Type);
            if (request.Request.Data != null) updates.Add("Data", request.Request.Data);
            if (request.Request.OrderIndex.HasValue) updates.Add("OrderIndex", request.Request.OrderIndex.Value);

            await contentRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Content updated successfully." });
        }
    }
}
