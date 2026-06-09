using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Comment.API.Features.Comments.AddComment
{
    public class AddCommentRequest
    {
        public string LessonId { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
    }

    public record AddCommentCommand(string Uid, AddCommentRequest Request) : ICommand<IResult>;

    public class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public AddCommentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var req = request.Request;
                var docRef = _firestoreDb.Collection("Comments").Document();
                
                var commentDict = new Dictionary<string, object>
                {
                    { "LessonId", req.LessonId },
                    { "ParentId", req.ParentId },
                    { "Content", req.Content },
                    { "UserId", request.Uid },
                    { "UserName", req.UserName },
                    { "UserRole", req.UserRole },
                    { "ProfileImageUrl", req.ProfileImageUrl },
                    { "CreatedAt", Timestamp.GetCurrentTimestamp() }
                };

                await docRef.SetAsync(commentDict, cancellationToken: cancellationToken);
                return Results.Ok(new { Id = docRef.Id });
            }
            catch (Exception ex)
            {
                return Results.Json(new { Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
