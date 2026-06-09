using Carter;
using MediatR;
using System.Security.Claims;

namespace Identity.API.Features.Users.DeleteAvatar
{
    public class DeleteAvatarEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/users/profile/avatar", async (ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new DeleteAvatarCommand(uid);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
