using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;

namespace Hangman.Services.UserService
{
    public interface IUserService
    {
        void DeleteUser(string username);
        string GetMyName();
        Task<User> GetUserAsync(string username);
        IEnumerable<User> GetUsers();
        UserRegisterResponseDto RegisterUser(UserRegisterRequestDto request);
        Task<User> SetTokenToUserAsync(string username, RefreshToken newRefreshToken);
    }
}
