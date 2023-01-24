using Hangman.Models;
using Hangman.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Services.AuthManager
{
    public interface IAuthManager
    {
        Task<string> SetToken(User user, HttpResponse Response);
        Task<User> VerifyUser(UserLoginRequestDto request);
        Task<bool?> CheckTokenValidation(HttpRequest Request);
    }
}
