using Carter;
using Identity.API.Models;
using MediatR;

namespace Identity.API.Features.Users.ChangeRole
{
    public class ChangeRoleEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/users/{id}/role", async (string id, ChangeRoleRequest req, ISender sender) =>
            {
                var cmd = new ChangeRoleCommand(id, req.Role);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
