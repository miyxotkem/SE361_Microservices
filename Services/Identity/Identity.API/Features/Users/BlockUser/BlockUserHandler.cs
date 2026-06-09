using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.BlockUser
{
    public record BlockUserCommand(string Id, bool IsBlocked) : ICommand<IResult>;

    public class BlockUserCommandHandler : ICommandHandler<BlockUserCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public BlockUserCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Id);
            var doc = await docRef.GetSnapshotAsync(cancellationToken);
            if (!doc.Exists) return Results.NotFound(new { Message = "User not found." });

            await docRef.UpdateAsync("IsBlocked", request.IsBlocked, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = $"User {(request.IsBlocked ? "blocked" : "unblocked")} successfully." });
        }
    }
}
