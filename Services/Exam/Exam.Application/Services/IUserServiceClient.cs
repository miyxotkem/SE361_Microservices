using System.Threading.Tasks;

namespace Exam.Application.Services
{
    public interface IUserServiceClient
    {
        Task<string> GetUserFullNameAsync(string userId);
    }
}
