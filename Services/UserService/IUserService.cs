using Hangman.Models;

namespace Hangman.Services.UserService
{
    public interface IUserService
    {
        string GetMyName();
        User GetUser(string username);
    }
}
