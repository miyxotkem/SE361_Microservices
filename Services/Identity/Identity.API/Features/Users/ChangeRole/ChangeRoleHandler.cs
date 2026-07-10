using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Identity.API.Features.Users.ChangeRole
{
    public record ChangeRoleCommand(string Id, string Role) : ICommand<IResult>;

    public class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand, IResult>
    {
        private readonly IdentityDbContext _context;
        private readonly IDistributedCache _cache;

        public ChangeRoleCommandHandler(IdentityDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IResult> Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });

            user.Role = request.Role;
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cache.RemoveAsync("GetAllUsers", cancellationToken);

            return Results.Ok(new { Message = "Role updated successfully." });
        }
    }
}
