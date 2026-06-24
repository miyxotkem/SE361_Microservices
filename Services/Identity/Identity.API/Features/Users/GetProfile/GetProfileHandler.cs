using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.GetProfile
{
    public record GetProfileQuery(string Uid) : IQuery<IResult>;

    public class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, IResult>
    {
        private readonly IdentityDbContext _context;

        public GetProfileQueryHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Uid, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User profile not found." });

            return Results.Ok(user);
        }
    }
}
