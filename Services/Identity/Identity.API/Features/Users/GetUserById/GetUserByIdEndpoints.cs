using Carter;
using MediatR;

namespace Identity.API.Features.Users.GetUserById
{
    public class GetUserByIdEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users/{id}", async (string id, ISender sender) =>
            {
                var query = new GetUserByIdQuery(id);
                return await sender.Send(query);
            }).AllowAnonymous();
        }
    }
}
