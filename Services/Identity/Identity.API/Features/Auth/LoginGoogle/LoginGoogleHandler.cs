using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Google.Apis.Auth;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Features.Auth.LoginGoogle
{
    public record LoginGoogleCommand(string FirebaseToken) : ICommand<IResult>;

    public class LoginGoogleCommandHandler : ICommandHandler<LoginGoogleCommand, IResult>
    {
        private readonly IConfiguration _config;
        private readonly IdentityDbContext _context;
        private const string GoogleClientId = "105514257729-ienl99san19bis48vav5lppchd7fuf1j.apps.googleusercontent.com";

        public LoginGoogleCommandHandler(IConfiguration config, IdentityDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<IResult> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { GoogleClientId }
                };

                // Validate the Google ID Token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.FirebaseToken, settings);
                if (payload == null || string.IsNullOrEmpty(payload.Email))
                {
                    return Results.Json(new { Message = "Xác thực với Google thất bại." }, statusCode: StatusCodes.Status401Unauthorized);
                }

                string email = payload.Email.Trim().ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
                string role = "Student";

                if (user == null)
                {
                    user = new User
                    {
                        Id = payload.Subject, // Use Google Subject ID as Unique Identifier
                        Email = payload.Email,
                        FullName = payload.Name ?? payload.Email,
                        Role = "Student",
                        CreatedAt = DateTime.UtcNow,
                        IsBlocked = false,
                        ProfileImageUrl = payload.Picture
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    role = user.Role;
                    if (user.IsBlocked)
                    {
                        return Results.BadRequest(new { Message = "Tài khoản của bạn đã bị khóa bởi Admin!" });
                    }

                    // Sync photo URL if missing
                    if (string.IsNullOrEmpty(user.ProfileImageUrl) && !string.IsNullOrEmpty(payload.Picture))
                    {
                        user.ProfileImageUrl = payload.Picture;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                var jwtToReturn = AuthHelper.GenerateJwtToken(user.Id, user.Email, role, _config);

                return Results.Ok(new
                {
                    Token = jwtToReturn,
                    Uid = user.Id,
                    Email = user.Email,
                    DisplayName = user.FullName,
                    FirebaseToken = request.FirebaseToken,
                    ExpiresIn = 86400
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Token Google không hợp lệ hoặc đã hết hạn.", Error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        }
    }
}
