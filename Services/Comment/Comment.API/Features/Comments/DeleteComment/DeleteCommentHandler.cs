using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Comment.API.Features.Comments.DeleteComment
{
    public record DeleteCommentCommand(string CommentId, string Uid, string UserRole) : ICommand<IResult>;

    public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public DeleteCommentCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var docRef = _firestoreDb.Collection("Comments").Document(request.CommentId);
                var docSnap = await docRef.GetSnapshotAsync(cancellationToken);
                
                if (!docSnap.Exists) return Results.NotFound("Bình luận không tồn tại");
                
                string authorId = docSnap.GetValue<string>("UserId");
                
                if (request.UserRole == "Instructor" || request.UserRole == "Admin" || request.Uid == authorId)
                {
                    await docRef.DeleteAsync(cancellationToken: cancellationToken);
                    
                    // Cascade delete for replies
                    var replies = await _firestoreDb.Collection("Comments")
                        .WhereEqualTo("ParentId", request.CommentId)
                        .GetSnapshotAsync(cancellationToken);
                    
                    foreach (var reply in replies.Documents)
                    {
                        await reply.Reference.DeleteAsync(cancellationToken: cancellationToken);
                    }
                    return Results.Ok();
                }
                
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.Json(new { Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
