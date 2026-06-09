using Carter;
using Identity.API.Models;
using MediatR;

namespace Identity.API.Features.Users.BlockUser
{
    public class BlockUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/users/{id}/block", async (string id, BlockUserRequest req, ISender sender) =>
            {
                var cmd = new BlockUserCommand(id, req.IsBlocked);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
