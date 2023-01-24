using Hangman.Models;
using Hangman.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Extensions
{
    public interface IAuthManager
    {
        Task<string> SetToken(User user, HttpResponse Response);
        Task<object> VerifyUser(UserLoginRequestDto request);
        Task<int> CheckTokenValidation(HttpRequest Request);
    }
}
