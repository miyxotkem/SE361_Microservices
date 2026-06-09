using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Features.Auth
{
    public static class AuthHelper
    {
        public static string GenerateJwtToken(string uid, string email, string role, IConfiguration config)
        {
            var keyStr = config["Jwt:Key"] ?? "Edu_Smart_Secret_Key_At_Least_32_Bytes_Long";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, uid),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                config["Jwt:Issuer"] ?? "EduSmartAPI",
                config["Jwt:Audience"] ?? "EduSmartWPF",
                claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
