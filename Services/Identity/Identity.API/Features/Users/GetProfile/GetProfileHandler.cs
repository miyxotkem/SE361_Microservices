using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.GetProfile
{
    public record GetProfileQuery(string Uid) : IQuery<IResult>;

    public class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetProfileQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var doc = await _firestoreDb.Collection("Users").Document(request.Uid).GetSnapshotAsync(cancellationToken);
            if (!doc.Exists) return Results.NotFound(new { Message = "User profile not found." });

            return Results.Ok(UserHelper.ConvertFirestoreTypes(doc.ToDictionary()));
        }
    }
}
