using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.UpdateAvatar
{
    public record UpdateAvatarCommand(string Uid, string ProfileImageUrl) : ICommand<IResult>;

    public class UpdateAvatarCommandHandler : ICommandHandler<UpdateAvatarCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateAvatarCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Uid);
            await docRef.UpdateAsync("ProfileImageUrl", request.ProfileImageUrl, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Avatar updated successfully.", ProfileImageUrl = request.ProfileImageUrl });
        }
    }
}
