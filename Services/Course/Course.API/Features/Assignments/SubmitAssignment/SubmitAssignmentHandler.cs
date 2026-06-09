using BuildingBlocks.CQRS;
using Course.API.Models;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.SubmitAssignment
{
    public record SubmitAssignmentCommand(string CourseId, string AsmId, string Uid, SubmitAssignmentRequest Request) : ICommand<IResult>;

    public class SubmitAssignmentCommandHandler : ICommandHandler<SubmitAssignmentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public SubmitAssignmentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
        {
            var asmRef = _firestoreDb.Collection("Courses").Document(request.CourseId).Collection("Assignments").Document(request.AsmId);
            var asmSnap = await asmRef.GetSnapshotAsync(cancellationToken);
            if (!asmSnap.Exists) return Results.NotFound("Assignment not found.");

            bool isLate = false;
            var asmDict = asmSnap.ToDictionary();
            if (asmDict.ContainsKey("DueDate") && asmDict["DueDate"] != null)
            {
                if (asmDict["DueDate"] is Timestamp ts)
                {
                    isLate = DateTime.UtcNow > ts.ToDateTime().ToUniversalTime();
                }
                else if (DateTime.TryParse(asmDict["DueDate"].ToString(), out DateTime dueDate))
                {
                    isLate = DateTime.UtcNow > dueDate.ToUniversalTime();
                }
            }

            var subData = new Dictionary<string, object>
            {
                { "StudentId", request.Uid },
                { "FileUrl", request.Request.FileUrl },
                { "Content", request.Request.Content },
                { "SubmittedAt", DateTime.UtcNow },
                { "IsLate", isLate },
                { "Score", null! }
            };

            await asmRef.Collection("Submissions").Document(request.Uid).SetAsync(subData, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Assignment submitted successfully." });
        }
    }
}
