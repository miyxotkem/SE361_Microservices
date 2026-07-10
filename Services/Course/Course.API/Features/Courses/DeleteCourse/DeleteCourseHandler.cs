using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Course.API.Features.Courses.DeleteCourse
{
    public record DeleteCourseCommand(string Id) : ICommand<IResult>;

    public class DeleteCourseCommandHandler : ICommandHandler<DeleteCourseCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IDistributedCache _cache;

        public DeleteCourseCommandHandler(FirestoreDb firestoreDb, IDistributedCache cache)
        {
            _firestoreDb = firestoreDb;
            _cache = cache;
        }

        public async Task<IResult> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Courses").Document(request.Id);
            if (!(await docRef.GetSnapshotAsync(cancellationToken)).Exists) return Results.NotFound();

            var notifsSnap = await _firestoreDb.Collection("Notifications")
                .WhereEqualTo("CourseId", request.Id)
                .GetSnapshotAsync(cancellationToken);
            if (notifsSnap.Documents.Count > 0)
            {
                var batch = _firestoreDb.StartBatch();
                foreach (var doc in notifsSnap.Documents)
                {
                    batch.Delete(doc.Reference);
                }
                await batch.CommitAsync(cancellationToken);
            }

            await docRef.DeleteAsync(cancellationToken: cancellationToken);
            await _cache.RemoveAsync("all_courses", cancellationToken);
            
            return Results.Ok(new { Message = "Course deleted successfully." });
        }
    }
}
