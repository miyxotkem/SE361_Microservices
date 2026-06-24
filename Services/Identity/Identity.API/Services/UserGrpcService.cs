using Grpc.Core;
using Identity.API.Data;
using Identity.API.Grpc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Identity.API.Services
{
    public class UserGrpcService : UserProtoService.UserProtoServiceBase
    {
        private readonly IdentityDbContext _context;
        private readonly ILogger<UserGrpcService> _logger;

        public UserGrpcService(IdentityDbContext context, ILogger<UserGrpcService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<UserProfileModel> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetUserProfile called for UserId: {UserId}", request.UserId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, context.CancellationToken);

            if (user == null)
            {
                _logger.LogWarning("gRPC GetUserProfile - UserId {UserId} not found in PostgreSQL.", request.UserId);
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found."));
            }

            return new UserProfileModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? "Student",
                Email = user.Email ?? "",
                Role = user.Role ?? "Student"
            };
        }
    }
}
