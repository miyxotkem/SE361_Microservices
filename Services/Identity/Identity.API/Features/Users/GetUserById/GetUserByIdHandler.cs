using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.GetUserById
{
    public record GetUserByIdQuery(string Id) : IQuery<IResult>;

    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetUserByIdQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var doc = await _firestoreDb.Collection("Users").Document(request.Id).GetSnapshotAsync(cancellationToken);
            if (!doc.Exists) return Results.NotFound(new { Message = "User not found." });
            return Results.Ok(new { Id = doc.Id, Data = UserHelper.ConvertFirestoreTypes(doc.ToDictionary()) });
        }
    }
}
