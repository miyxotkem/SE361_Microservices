using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.DeleteAssignment
{
    public record DeleteAssignmentCommand(string CourseId, string AsmId) : ICommand<IResult>;

    public class DeleteAssignmentCommandHandler : ICommandHandler<DeleteAssignmentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteAssignmentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
        {
            var asmRef = _firestoreDb.Collection("Courses").Document(request.CourseId).Collection("Assignments").Document(request.AsmId);
            if (!(await asmRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Assignment not found.");

            await asmRef.DeleteAsync(cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Assignment deleted successfully." });
        }
    }
}
