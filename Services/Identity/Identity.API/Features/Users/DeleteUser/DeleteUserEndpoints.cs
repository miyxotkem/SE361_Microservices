using Carter;
using MediatR;

namespace Identity.API.Features.Users.DeleteUser
{
    public class DeleteUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/users/{id}", async (string id, ISender sender) =>
            {
                var cmd = new DeleteUserCommand(id);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
