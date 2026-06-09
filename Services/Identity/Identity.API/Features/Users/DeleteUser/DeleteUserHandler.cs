using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.DeleteUser
{
    public record DeleteUserCommand(string Id) : ICommand<IResult>;

    public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteUserCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Id);
            var snap = await docRef.GetSnapshotAsync(cancellationToken);
            if (!snap.Exists) return Results.NotFound("User not found");

            await docRef.DeleteAsync(cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "User deleted successfully" });
        }
    }
}
