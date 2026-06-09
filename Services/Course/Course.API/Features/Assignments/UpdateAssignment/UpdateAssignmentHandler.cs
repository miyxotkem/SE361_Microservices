using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.UpdateAssignment
{
    public record UpdateAssignmentCommand(string CourseId, string AsmId, UpdateAssignmentRequest Request) : ICommand<IResult>;

    public class UpdateAssignmentCommandHandler : ICommandHandler<UpdateAssignmentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateAssignmentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var asmRef = _firestoreDb.Collection("Courses").Document(request.CourseId).Collection("Assignments").Document(request.AsmId);
            if (!(await asmRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Assignment not found.");

            var updates = new Dictionary<string, object>();
            if (request.Request.Title != null) updates.Add("Title", request.Request.Title);
            if (request.Request.Description != null) updates.Add("Description", request.Request.Description);
            if (request.Request.Deadline != default) updates.Add("Deadline", request.Request.Deadline.ToUniversalTime());
            if (request.Request.AttachedFileUrl != null) updates.Add("AttachedFileUrl", request.Request.AttachedFileUrl);
            updates.Add("UpdatedAt", DateTime.UtcNow);

            await asmRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Assignment updated successfully." });
        }
    }
}
