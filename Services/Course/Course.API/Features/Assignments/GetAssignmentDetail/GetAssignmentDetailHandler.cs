using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Assignments.GetAssignmentDetail
{
    public record GetAssignmentDetailQuery(string CourseId, string AsmId) : IQuery<IResult>;

    public class GetAssignmentDetailQueryHandler : IQueryHandler<GetAssignmentDetailQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetAssignmentDetailQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetAssignmentDetailQuery request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Courses").Document(request.CourseId)
                                     .Collection("Assignments").Document(request.AsmId);
            var docSnap = await docRef.GetSnapshotAsync(cancellationToken);
            if (!docSnap.Exists) return Results.NotFound(new { Message = "Assignment not found." });

            return Results.Ok(new
            {
                Id = docSnap.Id,
                Data = CourseHelper.ConvertFirestoreTypes(docSnap.ToDictionary())
            });
        }
    }
}
