using System;
using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Comment.API.Features.Comments.GetComments
{
    public record GetCommentsQuery(string LessonId) : ICachedQuery<IResult>
    {
        public string CacheKey => $"GetComments-{LessonId}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
    public class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetCommentsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
        {
            var snap = await _firestoreDb.Collection("Comments")
                .WhereEqualTo("LessonId", request.LessonId)
                .GetSnapshotAsync(cancellationToken);

            var comments = snap.Documents.Select(d => {
                var dict = d.ToDictionary();
                if (dict.TryGetValue("CreatedAt", out var createdAtObj) && createdAtObj is Timestamp ts)
                {
                    dict["CreatedAt"] = ts.ToDateTime().ToString("o");
                }
                return new
                {
                    Id = d.Id,
                    Data = dict
                };
            })
            .OrderByDescending(c => c.Data.ContainsKey("CreatedAt") ? c.Data["CreatedAt"]?.ToString() ?? "" : "")
            .ToList();

            return Results.Ok(comments);
        }
    }
}
