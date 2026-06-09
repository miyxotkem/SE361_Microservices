using Carter;
using MediatR;
using System.Security.Claims;

namespace Comment.API.Features.Comments.DeleteComment
{
    public class DeleteCommentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/comments/{commentId}", async (string commentId, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                string role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var cmd = new DeleteCommentCommand(commentId, uid, role);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
