using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Contents.AddCourseContent
{
    public record AddCourseContentCommand(string CourseId, CreateCourseContentRequest Request) : ICommand<IResult>;

    public class AddCourseContentCommandHandler : ICommandHandler<AddCourseContentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public AddCourseContentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(AddCourseContentCommand request, CancellationToken cancellationToken)
        {
            var courseRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            if (!(await courseRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Course not found.");

            var contentData = new Dictionary<string, object>
            {
                { "CourseId", request.CourseId },
                { "Title", request.Request.Title },
                { "Type", request.Request.Type },
                { "Data", request.Request.Data },
                { "OrderIndex", request.Request.OrderIndex },
                { "CreatedAt", DateTime.UtcNow }
            };

            var contentRef = await courseRef.Collection("Contents").AddAsync(contentData, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Content added successfully.", Id = contentRef.Id });
        }
    }
}
