using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.DeleteAvatar
{
    public record DeleteAvatarCommand(string Uid) : ICommand<IResult>;

    public class DeleteAvatarCommandHandler : ICommandHandler<DeleteAvatarCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteAvatarCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Uid);
            await docRef.UpdateAsync("ProfileImageUrl", "", cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Avatar deleted successfully." });
        }
    }
}
