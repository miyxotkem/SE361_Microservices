using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Auth.Register
{
    public record RegisterCommand(string Email, string Password, string FullName) : ICommand<IResult>;

    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, IResult>
    {
        private readonly IdentityDbContext _context;

        public RegisterCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var emailLower = request.Email.Trim().ToLower();
                var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower, cancellationToken);
                if (emailExists)
                {
                    return Results.BadRequest(new { Message = "Email này đã được sử dụng bởi tài khoản khác." });
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                string newUid = Guid.NewGuid().ToString();

                var newUser = new User
                {
                    Id = newUid,
                    Email = request.Email.Trim(),
                    PasswordHash = passwordHash,
                    FullName = request.FullName,
                    Role = "Student",
                    CreatedAt = DateTime.UtcNow,
                    IsBlocked = false
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                return Results.Ok(new { Message = "Đăng ký thành công", Uid = newUid });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Lỗi đăng ký", Error = ex.Message });
            }
        }
    }
}
