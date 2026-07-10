using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Identity.API.Features.Users.DeleteUser
{
    public record DeleteUserCommand(string Id) : ICommand<IResult>;

    public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, IResult>
    {
        private readonly IdentityDbContext _context;
        private readonly IDistributedCache _cache;

        public DeleteUserCommandHandler(IdentityDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null) return Results.NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync("GetAllUsers", cancellationToken);

            return Results.Ok(new { Message = "User deleted successfully" });
        }
    }
}
