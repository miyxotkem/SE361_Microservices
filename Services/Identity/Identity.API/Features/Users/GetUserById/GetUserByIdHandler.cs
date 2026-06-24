using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.GetUserById
{
    public record GetUserByIdQuery(string Id) : IQuery<IResult>;

    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, IResult>
    {
        private readonly IdentityDbContext _context;

        public GetUserByIdQueryHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });
            return Results.Ok(new { Id = user.Id, Data = user });
        }
    }
}
