using System;
using System.Threading.Tasks;
using Identity.API.Grpc;
using Exam.Application.Services;

namespace Exam.Infrastructure.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly UserProtoService.UserProtoServiceClient _client;

        public UserServiceClient(UserProtoService.UserProtoServiceClient client)
        {
            _client = client;
        }

        public async Task<string> GetUserFullNameAsync(string userId)
        {
            try
            {
                var response = await _client.GetUserProfileAsync(new GetUserProfileRequest
                {
                    UserId = userId
                });
                return response.FullName;
            }
            catch
            {
                return "Student";
            }
        }
    }
}
