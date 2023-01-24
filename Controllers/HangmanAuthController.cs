using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Hangman.Models;
using Hangman.Models.Exceptions;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Services.AuthManager;
using Hangman.Services.SessionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Controllers
{

    [Route("/api/HangmanAuthApi")]
    [ApiController]
    public class HangmanAuthController : Controller
    {

        #region Constructor

        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IValidator<UserLoginRequestDto> _userLoginValidator;
        private readonly IValidator<UserRegisterRequestDto> _userRegisterValidator;
        private readonly IAuthManager _authManager;


        public HangmanAuthController( IUserService userService,ISessionService sessionService,
            IValidator<UserLoginRequestDto> userLoginValidator, IValidator<UserRegisterRequestDto> userRegisterValidator,
            IAuthManager authManager)
        {
            _userService = userService;
            _sessionService = sessionService;
            _userLoginValidator = userLoginValidator;
            _userRegisterValidator = userRegisterValidator;
            _authManager = authManager;
        }
        #endregion

        #region GetAllUsers

        [HttpGet("Users"),Authorize(Roles ="Admin")]

        public async  Task<ActionResult<List<User>>> GetUsers()
        {
            await _authManager.CheckTokenValidation(Request);

            var list = _userService.GetUsers();

            if (!list.Any())
            {
                throw new UserNotFoundException();
            }

            return Ok(list);

            
        }


        #endregion

        #region DeleteUser

        [HttpDelete("DeleteUser", Name = "DeleteUserByUsername"),Authorize(Roles ="Admin")]
        public async Task<ActionResult<bool>> DeleteUserByUsername(string username)
        {
            await _authManager.CheckTokenValidation(Request);


            if (!await _userService.UserExists(username))
            {
                throw new UserNotFoundException(username);

            }
            _userService.DeleteUser(username);
            _sessionService.DeleteSessionsOfUser(username);

            return Ok($"User {username} Deleted! And All Sessions of the {username} Deleted!");

        }

        #endregion

        #region GetMe

        [HttpGet("GetMe"), Authorize]
        public async Task<ActionResult<string>> GetMe()
        {
            await _authManager.CheckTokenValidation(Request);

            return Ok(_userService.GetMyName());
        }

        #endregion

        #region UserRegister

        [HttpPost("Register")]
        public async Task<ActionResult<UserRegisterResponseDto>> Register(UserRegisterRequestDto request)
        {

            ValidationResult result = await _userRegisterValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                throw new InvalidRequestException(result);
            }

            if (await _userService.UserExists(request.Username))
            {
                throw new UserAlreadyExistsException(request.Username);
            }

            
            var response = _userService.RegisterUser(request);

            return Ok(response);
        }

        #endregion

        #region UserLogin

        [HttpPost("Login")]
        public async Task<ActionResult<UserLoginResponseDto>> Login(UserLoginRequestDto request)
        {

            ValidationResult result = await _userLoginValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                throw new InvalidRequestException(result);
            }

            User VerifiedUser = await _authManager.VerifyUser(request);

            var Token = await _authManager.SetToken(VerifiedUser,Response);

            return Ok(Token);
        }



        #endregion

        #region RefreshToken

        [HttpPost("RefreshToken"),Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {

            await _authManager.CheckTokenValidation(Request);

            var userName = _userService.GetMyName();

            var user = await _userService.GetUserAsync(userName);

            var token = await _authManager.SetToken(user,Response);

            return Ok(token);
        }

        #endregion


    }
}
