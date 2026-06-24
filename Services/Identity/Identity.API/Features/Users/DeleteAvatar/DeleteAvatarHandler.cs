using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.DeleteAvatar
{
    public record DeleteAvatarCommand(string Uid) : ICommand<IResult>;

    public class DeleteAvatarCommandHandler : ICommandHandler<DeleteAvatarCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public DeleteAvatarCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Uid, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });

            user.ProfileImageUrl = "";
            await _context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { Message = "Avatar deleted successfully." });
        }
    }
}
