using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Contents.GetCourseContents
{
    public record GetCourseContentsQuery(string CourseId) : IQuery<IResult>;

    public class GetCourseContentsQueryHandler : IQueryHandler<GetCourseContentsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetCourseContentsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetCourseContentsQuery request, CancellationToken cancellationToken)
        {
            var snap = await _firestoreDb.Collection("Courses").Document(request.CourseId)
                                         .Collection("Contents").OrderBy("OrderIndex").GetSnapshotAsync(cancellationToken);
            var contents = snap.Documents.Select(d => new { Id = d.Id, Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary()) });
            return Results.Ok(contents);
        }
    }
}
