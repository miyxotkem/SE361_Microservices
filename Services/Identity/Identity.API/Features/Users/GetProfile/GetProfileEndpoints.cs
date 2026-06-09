using Carter;
using MediatR;
using System.Security.Claims;

namespace Identity.API.Features.Users.GetProfile
{
    public class GetProfileEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users/profile", async (ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var query = new GetProfileQuery(uid);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
