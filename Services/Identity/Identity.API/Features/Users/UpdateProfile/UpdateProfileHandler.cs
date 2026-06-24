using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Users.UpdateProfile
{
    public record UpdateProfileCommand(string Uid, string FullName, string? ProfileImageUrl) : ICommand<IResult>;

    public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public UpdateProfileCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Uid, cancellationToken);
            if (user == null) return Results.NotFound(new { Message = "User not found." });

            user.FullName = request.FullName;
            if (request.ProfileImageUrl != null)
            {
                user.ProfileImageUrl = request.ProfileImageUrl;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { Message = "Profile updated successfully." });
        }
    }
}
