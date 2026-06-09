using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Registrations.ApproveRegistration
{
    public record ApproveRegistrationCommand(string CourseId, string RegId) : ICommand<IResult>;

    public class ApproveRegistrationCommandHandler : ICommandHandler<ApproveRegistrationCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public ApproveRegistrationCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(ApproveRegistrationCommand request, CancellationToken cancellationToken)
        {
            var regRef = _firestoreDb.Collection("courseRegistrations").Document(request.RegId);
            if (!(await regRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Registration not found.");

            var updates = new Dictionary<string, object>
            {
                { "status", "accepted" },
                { "approvedDate", FieldValue.ServerTimestamp }
            };
            await regRef.UpdateAsync(updates, cancellationToken: cancellationToken);

            return Results.Ok(new { Message = "Registration approved." });
        }
    }
}
