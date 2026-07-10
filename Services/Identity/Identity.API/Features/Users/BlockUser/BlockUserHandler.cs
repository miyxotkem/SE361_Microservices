using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Identity.API.Features.Users.BlockUser
{
    public record BlockUserCommand(string Id, bool IsBlocked) : ICommand<IResult>;

    public class BlockUserCommandHandler : ICommandHandler<BlockUserCommand, IResult>
    {
        private readonly IdentityDbContext _context;
        private readonly IDistributedCache _cache;

        public BlockUserCommandHandler(IdentityDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IResult> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });

            user.IsBlocked = request.IsBlocked;
            await _context.SaveChangesAsync(cancellationToken);
            
            await _cache.RemoveAsync("GetAllUsers", cancellationToken);

            return Results.Ok(new { Message = $"User {(request.IsBlocked ? "blocked" : "unblocked")} successfully." });
        }
    }
}
