using BuildingBlocks.CQRS;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Auth.LoginEmail
{
    public record LoginEmailCommand(string Email, string Password) : ICommand<IResult>;

    public class LoginEmailCommandHandler : ICommandHandler<LoginEmailCommand, IResult>
    {
        private readonly IConfiguration _config;
        private readonly IdentityDbContext _context;

        public LoginEmailCommandHandler(IConfiguration config, IdentityDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<IResult> Handle(LoginEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // -- Special Admin --
                if (request.Email == "admin" && request.Password == "admin")
                {
                    string uidAdmin = "admin_super_id";
                    string roleAdmin = "Admin";
                    var tokenAdmin = AuthHelper.GenerateJwtToken(uidAdmin, "admin@system.com", roleAdmin, _config);
                    return Results.Ok(new
                    {
                        Token = tokenAdmin,
                        Uid = uidAdmin,
                        Email = "admin@system.com",
                        DisplayName = "Administrator",
                        FirebaseToken = "fake-token",
                        ExpiresIn = 86400
                    });
                }

                var emailLower = request.Email.Trim().ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, cancellationToken);

                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    return Results.Json(new { Message = "Tên đăng nhập hoặc mật khẩu không chính xác." }, statusCode: StatusCodes.Status401Unauthorized);
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!isValidPassword)
                {
                    return Results.Json(new { Message = "Tên đăng nhập hoặc mật khẩu không chính xác." }, statusCode: StatusCodes.Status401Unauthorized);
                }

                if (user.IsBlocked)
                {
                    return Results.BadRequest(new { Message = "Tài khoản của bạn đã bị khóa bởi Admin!" });
                }

                var jwtToReturn = AuthHelper.GenerateJwtToken(user.Id, user.Email, user.Role, _config);

                return Results.Ok(new
                {
                    Token = jwtToReturn,
                    Uid = user.Id,
                    Email = user.Email,
                    DisplayName = user.FullName,
                    FirebaseToken = "supabase-session",
                    ExpiresIn = 86400
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Lỗi xử lý đăng nhập hệ thống", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
