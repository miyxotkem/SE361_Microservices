using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Registrations.RejectRegistration
{
    public record RejectRegistrationCommand(string CourseId, string RegId) : ICommand<IResult>;

    public class RejectRegistrationCommandHandler : ICommandHandler<RejectRegistrationCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public RejectRegistrationCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(RejectRegistrationCommand request, CancellationToken cancellationToken)
        {
            var regRef = _firestoreDb.Collection("courseRegistrations").Document(request.RegId);
            if (!(await regRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound("Registration not found.");

            await regRef.UpdateAsync("status", "rejected", cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Registration rejected." });
        }
    }
}
