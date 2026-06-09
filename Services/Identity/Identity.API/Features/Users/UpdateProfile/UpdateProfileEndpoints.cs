using Carter;
using Identity.API.Models;
using MediatR;
using System.Security.Claims;

namespace Identity.API.Features.Users.UpdateProfile
{
    public class UpdateProfileEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/users/profile", async (UpdateProfileRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new UpdateProfileCommand(uid, req.FullName, req.ProfileImageUrl);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }
    }
}
