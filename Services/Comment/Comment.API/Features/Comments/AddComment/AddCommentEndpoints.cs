using Carter;
using MediatR;
using System.Security.Claims;

namespace Comment.API.Features.Comments.AddComment
{
    public class AddCommentEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments", async (AddCommentRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new AddCommentCommand(uid, req);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
