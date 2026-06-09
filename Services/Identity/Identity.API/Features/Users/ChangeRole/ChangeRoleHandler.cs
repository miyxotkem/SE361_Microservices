using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.ChangeRole
{
    public record ChangeRoleCommand(string Id, string Role) : ICommand<IResult>;

    public class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public ChangeRoleCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Users").Document(request.Id);
            var doc = await docRef.GetSnapshotAsync(cancellationToken);
            if (!doc.Exists) return Results.NotFound(new { Message = "User not found." });

            await docRef.UpdateAsync("Role", request.Role, cancellationToken: cancellationToken);
            return Results.Ok(new { Message = "Role updated successfully." });
        }
    }
}
