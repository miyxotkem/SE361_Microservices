using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.UpdateProfile
{
    public record UpdateProfileCommand(string Uid, string FullName, string? ProfileImageUrl) : ICommand<IResult>;

    public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateProfileCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Uid);
            var updates = new Dictionary<string, object>
            {
                { "FullName", request.FullName }
            };

            if (request.ProfileImageUrl != null)
            {
                updates["ProfileImageUrl"] = request.ProfileImageUrl;
            }

            await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Profile updated successfully." });
        }
    }
}
