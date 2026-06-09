using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Assignments.GetSubmissions
{
    public record GetSubmissionsQuery(string CourseId, string AsmId, string Uid, bool IsStudent) : IQuery<IResult>;

    public class GetSubmissionsQueryHandler : IQueryHandler<GetSubmissionsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetSubmissionsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var submissionsCol = _firestoreDb.Collection("Courses").Document(request.CourseId)
                                            .Collection("Assignments").Document(request.AsmId)
                                            .Collection("Submissions");

            if (request.IsStudent)
            {
                var subDoc = await submissionsCol.Document(request.Uid).GetSnapshotAsync(cancellationToken);
                if (subDoc.Exists)
                {
                    return Results.Ok(new[] { new { Id = subDoc.Id, Data = CourseHelper.ConvertFirestoreTypes(subDoc.ToDictionary()) } });
                }
                return Results.Ok(new object[] { });
            }
            else
            {
                var subSnap = await submissionsCol.GetSnapshotAsync(cancellationToken);
                var submissions = subSnap.Documents.Select(d => new { Id = d.Id, Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary()) });
                return Results.Ok(submissions);
            }
        }
    }
}
