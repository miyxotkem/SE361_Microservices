using Carter;
using Exam.Application.Exams.Queries.GetMyHistory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Exam.API.Endpoints
{
    public class GetMyHistory : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/exams/my-history", async (ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var query = new GetMyHistoryQuery(uid);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
