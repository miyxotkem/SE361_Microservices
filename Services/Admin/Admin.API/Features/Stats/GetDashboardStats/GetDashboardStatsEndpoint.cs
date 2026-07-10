using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Admin.API.Features.Stats.GetDashboardStats
{
    public record GetDashboardStatsQuery() : IRequest<GetDashboardStatsResult>;
    public record GetDashboardStatsResult(int TotalUsers, int TotalCourses, int TotalRevenue);

    public class GetDashboardStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/stats", async (ISender sender) =>
            {
                var result = await sender.Send(new GetDashboardStatsQuery());
                return Results.Ok(result);
            })
            .WithName("GetDashboardStats")
            .Produces<GetDashboardStatsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Dashboard Stats")
            .WithDescription("Get dashboard statistics for admin");
        }
    }

    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, GetDashboardStatsResult>
    {
        public async Task<GetDashboardStatsResult> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            // TODO: Fetch real stats from other services or database
            return await Task.FromResult(new GetDashboardStatsResult(TotalUsers: 1500, TotalCourses: 45, TotalRevenue: 50000));
        }
    }
}
