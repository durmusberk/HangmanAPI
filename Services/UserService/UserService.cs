using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;


namespace Hangman.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork = new();


        public UserService(IHttpContextAccessor httpContextAccessor,IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        //generic repository
        //unit of work

        public  void DeleteUser(string username)
        {
            _unitOfWork.UserRepository.Delete(username);
            _unitOfWork.SaveAsync();
            
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

        public async Task<User> GetUserAsync(string username)
        {
            var user = await _unitOfWork.UserRepository.GetByID(username);

            if (user == null)
            {
                return null;//there should be exception throw
            }
            
            return user;
        }

        public IEnumerable<User> GetUsers()
        {
            return _unitOfWork.UserRepository.Get();
        }

        public async Task<bool> UserExists(string username)
        {
            return  await _unitOfWork.UserRepository.GetByID(username) != null;
        }

        public  UserRegisterResponseDto RegisterUser(UserRegisterRequestDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var new_user = new User
            {
                Username = request.Username.ToLower(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = request.Role,
                CreatedDate = DateTime.Now
            };

            
            _unitOfWork.UserRepository.InsertAsync(new_user);

            _unitOfWork.SaveAsync();

            var response = _mapper.Map<UserRegisterResponseDto>(new_user);

            return response;
        }

        public async Task<User> SetTokenToUserAsync(string username,RefreshToken newRefreshToken)
        {
            var user = await GetUserAsync(username);
            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
            
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.SaveAsync();
            
            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
