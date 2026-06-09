using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Registrations.CancelRegistration
{
    public record CancelRegistrationCommand(string CourseId, string Uid) : ICommand<IResult>;

    public class CancelRegistrationCommandHandler : ICommandHandler<CancelRegistrationCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public CancelRegistrationCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
        {
            string regId = $"{request.Uid}_{request.CourseId}";
            await _firestoreDb.Collection("courseRegistrations").Document(regId).DeleteAsync(cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Registration cancelled." });
        }
    }
}
