using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.GradeSubmission
{
    public record GradeSubmissionCommand(string CourseId, string AsmId, string StudentId, GradeSubmissionRequest Request) : ICommand<IResult>;

    public class GradeSubmissionCommandHandler : ICommandHandler<GradeSubmissionCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GradeSubmissionCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
        {
            var subRef = _firestoreDb.Collection("Courses").Document(request.CourseId)
                                     .Collection("Assignments").Document(request.AsmId)
                                     .Collection("Submissions").Document(request.StudentId);
            
            if (!(await subRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Submission not found.");

            var updates = new Dictionary<string, object>
            {
                { "Score", request.Request.Score! },
                { "Comment", request.Request.Comment ?? "" }
            };

            await subRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Submission graded successfully." });
        }
    }
}
