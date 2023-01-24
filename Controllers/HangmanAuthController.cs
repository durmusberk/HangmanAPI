using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Hangman.Data;
using Hangman.Extensions;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Services.SessionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
            switch (await _authManager.CheckTokenValidation(Request))
            {
                case 401:
                    return Unauthorized();
                case 404:
                    return NotFound();
                case 400:
                    return BadRequest();
                default:
                    break;
            }

            var list = _userService.GetUsers();
            

            return Ok(list);
        }


        #endregion

        #region DeleteUser

        [HttpDelete("DeleteUser", Name = "DeleteUserByUsername"),Authorize(Roles ="Admin")]
        public async Task<ActionResult<bool>> DeleteUserByUsername(string username)
        {
            switch (await _authManager.CheckTokenValidation(Request))
            {
                case 401:
                    return Unauthorized();
                case 404:
                    return NotFound();
                case 400:
                    return BadRequest();
                default:
                    break;
            }

           
            if (await _userService.UserExists(username))
            {
             _userService.DeleteUser(username);
            _sessionService.DeleteSessionsOfUser(username);
                
            return Ok("User Deleted!");

            }
            return BadRequest("No Such A User!");

        }

        #endregion

        #region GetMe

        [HttpGet("GetMe"), Authorize]
        public async Task<ActionResult<string>> GetMe()
        {
            switch (await _authManager.CheckTokenValidation(Request))
            {
                case 401:
                    return Unauthorized();
                case 404:
                    return NotFound();
                case 400:
                    return BadRequest();
                default:
                    break;
            }

            
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
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }

            if (await _userService.UserExists(request.Username))
            {
                ModelState.AddModelError("UserAlreadyExistError", "This username already in use. Try Another!");
                return BadRequest(ModelState);
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
            if (!result.IsValid){ result.AddToModelState(ModelState); return BadRequest(ModelState);}
            
            var VerifiedUser = await _authManager.VerifyUser(request);

            switch (VerifiedUser)
            {
                case "1":
                    return BadRequest("No Such A User!");
                case "2":
                    return BadRequest("Wrong Password!");
                default:
                    break;
            }
           
            var Token = await _authManager.SetToken((User)VerifiedUser,Response);

            return Ok(Token);
        }



        #endregion

        #region RefreshToken

        [HttpPost("RefreshToken"),Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            
            switch (await _authManager.CheckTokenValidation(Request))
            {
                case 401:
                    return Unauthorized();
                case 404:
                    return NotFound();
                case 400:
                    return BadRequest();
                default:
                    break;
            }

            var userName = _userService.GetMyName();

            var user = await _userService.GetUserAsync(userName);

            var token = await _authManager.SetToken(user,Response);

            return Ok(token);
        }

        #endregion


    }
}
