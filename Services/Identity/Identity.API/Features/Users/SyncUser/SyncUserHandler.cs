using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.SyncUser
{
    public record SyncUserCommand(string Uid, string FullName, string Email, string? PhotoUrl, string? Provider) : ICommand<IResult>;

    public class SyncUserCommandHandler : ICommandHandler<SyncUserCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public SyncUserCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(SyncUserCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Uid);
            var docSnap = await docRef.GetSnapshotAsync(cancellationToken);

            if (!docSnap.Exists)
            {
                var newUser = new Dictionary<string, object>
                {
                    { "Uid", request.Uid },
                    { "FullName", string.IsNullOrEmpty(request.FullName) ? "User" : request.FullName },
                    { "Email", request.Email },
                    { "Role", "Student" },
                    { "CreatedAt", DateTime.UtcNow },
                    { "IsBlocked", false },
                    { "Provider", string.IsNullOrEmpty(request.Provider) ? "email" : request.Provider },
                    { "ProfileImageUrl", request.PhotoUrl ?? "" }
                };
                await docRef.SetAsync(newUser, cancellationToken: cancellationToken);
                return Results.Ok(new { Message = "User synchronized.", User = newUser });
            }

            return Results.Ok(new { Message = "User already exists.", User = UserHelper.ConvertFirestoreTypes(docSnap.ToDictionary()) });
        }
    }
}
