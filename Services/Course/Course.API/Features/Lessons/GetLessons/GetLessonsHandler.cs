using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Lessons.GetLessons
{
    public record GetLessonsQuery(string CourseId) : IQuery<IResult>;

    public class GetLessonsQueryHandler : IQueryHandler<GetLessonsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetLessonsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
        {
            var snap = await _firestoreDb.Collection("Lessons")
                .WhereEqualTo("CourseId", request.CourseId)
                .GetSnapshotAsync(cancellationToken);

            var lessons = snap.Documents
                .Select(d => new { Id = d.Id, Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary()) })
                .OrderBy(l => l.Data.ContainsKey("CreatedAt") ? l.Data["CreatedAt"] : null)
                .ToList();

            return Results.Ok(lessons);
        }
    }
}
