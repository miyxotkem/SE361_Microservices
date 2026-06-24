using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.SyncUser
{
    public record SyncUserCommand(string Uid, string FullName, string Email, string? PhotoUrl, string? Provider) : ICommand<IResult>;

    public class SyncUserCommandHandler : ICommandHandler<SyncUserCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public SyncUserCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(SyncUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Uid, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Id = request.Uid,
                    Email = request.Email,
                    FullName = string.IsNullOrEmpty(request.FullName) ? "User" : request.FullName,
                    Role = "Student",
                    CreatedAt = DateTime.UtcNow,
                    IsBlocked = false,
                    ProfileImageUrl = request.PhotoUrl ?? ""
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
                return Results.Ok(new { Message = "User synchronized.", User = user });
            }

            return Results.Ok(new { Message = "User already exists.", User = user });
        }
    }
}
