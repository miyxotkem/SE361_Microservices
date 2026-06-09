using Carter;
using Identity.API.Models;
using MediatR;
using System.Security.Claims;

namespace Identity.API.Features.Users.UpdateAvatar
{
    public class UpdateAvatarEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/users/profile/avatar", async (UpdateAvatarRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new UpdateAvatarCommand(uid, req.ProfileImageUrl);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
