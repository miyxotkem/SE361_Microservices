using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.CreateAssignment
{
    public record CreateAssignmentCommand(string CourseId, CreateAssignmentRequest Request) : ICommand<IResult>;

    public class CreateAssignmentCommandHandler : ICommandHandler<CreateAssignmentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public CreateAssignmentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var courseRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            if (!(await courseRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Course not found.");

            var asmData = new Dictionary<string, object>
            {
                { "Title", request.Request.Title },
                { "Description", request.Request.Description },
                { "DueDate", request.Request.DueDate.ToUniversalTime() },
                { "AttachedFileUrl", request.Request.AttachedFileUrl ?? "" },
                { "CreatedAt", DateTime.UtcNow }
            };

            var asmRef = await courseRef.Collection("Assignments").AddAsync(asmData, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Assignment created successfully.", Id = asmRef.Id });
        }
    }
}
