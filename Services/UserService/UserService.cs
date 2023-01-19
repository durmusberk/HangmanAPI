using System.Security.Claims;
using Hangman.Data;
using Hangman.Models;

namespace Hangman.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        public UserService(IHttpContextAccessor httpContextAccessor,ApplicationDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }

        public User GetUser(string username)
        {
            return _db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }
    }
}
