using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.UpdateAvatar
{
    public record UpdateAvatarCommand(string Uid, string ProfileImageUrl) : ICommand<IResult>;

    public class UpdateAvatarCommandHandler : ICommandHandler<UpdateAvatarCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public UpdateAvatarCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Uid, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });

            user.ProfileImageUrl = request.ProfileImageUrl;
            await _context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { Message = "Avatar updated successfully.", ProfileImageUrl = request.ProfileImageUrl });
        }
    }
}
