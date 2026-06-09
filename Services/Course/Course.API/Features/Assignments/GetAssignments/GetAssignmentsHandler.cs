using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Assignments.GetAssignments
{
    public record GetAssignmentsQuery(string CourseId) : IQuery<IResult>;

    public class GetAssignmentsQueryHandler : IQueryHandler<GetAssignmentsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetAssignmentsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
        {
            var asmSnap = await _firestoreDb.Collection("Courses").Document(request.CourseId)
                                            .Collection("Assignments").GetSnapshotAsync(cancellationToken);
            var assignments = asmSnap.Documents.Select(d => new { Id = d.Id, Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary()) });
            return Results.Ok(assignments);
        }
    }
}
