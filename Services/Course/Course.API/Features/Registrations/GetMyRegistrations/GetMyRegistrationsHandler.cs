using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Registrations.GetMyRegistrations
{
    public record GetMyRegistrationsQuery(string Uid) : IQuery<IResult>;

    public class GetMyRegistrationsQueryHandler : IQueryHandler<GetMyRegistrationsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetMyRegistrationsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetMyRegistrationsQuery request, CancellationToken cancellationToken)
        {
            var snapshot = await _firestoreDb.Collection("courseRegistrations")
                .WhereEqualTo("userId", request.Uid)
                .GetSnapshotAsync(cancellationToken);

            var registrations = snapshot.Documents.Select(d => new
            {
                Id = d.Id,
                Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary())
            });

            return Results.Ok(registrations);
        }
    }
}
