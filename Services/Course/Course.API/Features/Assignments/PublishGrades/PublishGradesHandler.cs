using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Assignments.PublishGrades
{
    public record PublishGradesCommand(string CourseId, string AsmId) : ICommand<IResult>;

    public class PublishGradesCommandHandler : ICommandHandler<PublishGradesCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public PublishGradesCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(PublishGradesCommand request, CancellationToken cancellationToken)
        {
            var asmRef = _firestoreDb.Collection("Courses").Document(request.CourseId)
                                     .Collection("Assignments").Document(request.AsmId);

            if (!(await asmRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Assignment not found.");

            await asmRef.UpdateAsync("IsGradesPublished", true, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Grades published successfully." });
        }
    }
}
