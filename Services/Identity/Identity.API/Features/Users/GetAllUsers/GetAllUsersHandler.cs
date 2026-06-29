using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.GetAllUsers
{
    public record GetAllUsersQuery() : ICachedQuery<IResult>
    {
        public string CacheKey => "GetAllUsers";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }

    public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IResult>
    {
        private readonly IdentityDbContext _context;

        public GetAllUsersQueryHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var usersList = await _context.Users.ToListAsync(cancellationToken);
                var users = usersList.Select(u => new
                {
                    Id = u.Id,
                    Data = u
                });

                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Lỗi khi lấy danh sách user từ Backend", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
