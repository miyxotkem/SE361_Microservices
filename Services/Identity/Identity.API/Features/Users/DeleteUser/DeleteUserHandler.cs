using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.DeleteUser
{
    public record DeleteUserCommand(string Id) : ICommand<IResult>;

    public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public DeleteUserCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null) return Results.NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { Message = "User deleted successfully" });
        }
    }
}
