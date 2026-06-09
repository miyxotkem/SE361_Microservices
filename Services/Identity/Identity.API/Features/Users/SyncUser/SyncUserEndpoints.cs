using Carter;
using MediatR;
using System.Security.Claims;

namespace Identity.API.Features.Users.SyncUser
{
    public class SyncUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/users/sync-user", async (SyncUserRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var cmd = new SyncUserCommand(uid, req.FullName, req.Email, req.PhotoUrl, req.Provider);
                return await sender.Send(cmd);
            }).RequireAuthorization();
        }

        public class SyncUserRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhotoUrl { get; set; }
            public string? Provider { get; set; }
        }
    }
}
