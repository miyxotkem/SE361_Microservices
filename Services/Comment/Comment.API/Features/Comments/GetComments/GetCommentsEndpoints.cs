using Carter;
using MediatR;

namespace Comment.API.Features.Comments.GetComments
{
    public class GetCommentsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/comments/lesson/{lessonId}", async (string lessonId, ISender sender) =>
            {
                var query = new GetCommentsQuery(lessonId);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
