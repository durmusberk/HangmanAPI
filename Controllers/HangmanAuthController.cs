using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Hangman.Data;
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

        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IValidator<UserLoginRequestDto> _userLoginValidator;
        private readonly IValidator<UserRegisterRequestDto> _userRegisterValidator;
        private readonly int JWTExpirationTimeInMinutes = 30;
        private readonly int RefreshTokenExpirationTimeInMinutes = 30;


        public HangmanAuthController(IConfiguration configuration, IUserService userService,ISessionService sessionService,IValidator<UserLoginRequestDto> userLoginValidator, IValidator<UserRegisterRequestDto> userRegisterValidator)
        {
            _configuration = configuration;
            _userService = userService;
            _sessionService = sessionService;
            _userLoginValidator = userLoginValidator;
            _userRegisterValidator = userRegisterValidator;
        }
        #endregion

        #region GetAllUsers

        [HttpGet("Users"),Authorize(Roles ="Admin")]

        public async  Task<ActionResult<List<User>>> GetUsers()
        {
            var valid = await CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }
            
            var list = _userService.GetUsers();
            

            return Ok(list);
        }


        #endregion

        #region DeleteUser

        [HttpDelete("DeleteUser", Name = "DeleteUserByUsername"),Authorize(Roles ="Admin")]
        public async Task<ActionResult<bool>> DeleteUserByUsername(string username)
        {
            var user = await _userService.GetUserAsync(username);
            if (user != null)
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
            var valid = await CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }
            var userName = _userService.GetMyName();
            return Ok(userName);
        }

        #endregion

        #region UserRegister

        [HttpPost("register")]
        public async Task<ActionResult<UserRegisterResponseDto>> Register(UserRegisterRequestDto request)
        {

            ValidationResult result = await _userRegisterValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }

            var user = await _userService.GetUserAsync(request.Username);

            if (user != null)
            {
                ModelState.AddModelError("Custom Error", "User Already Exists!");
                return BadRequest(ModelState);
            }

            
            var response = _userService.RegisterUser(request);

            return Ok(response);
        }

        #endregion

        #region UserLogin

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDto>> Login(UserLoginRequestDto request)
        {

            ValidationResult result = await _userLoginValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }


            var user = await _userService.GetUserAsync(request.Username);

            if (user == null)
            {
                ModelState.AddModelError("Custom Error", "User Does not Exists!");
                return BadRequest(ModelState);
            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is wrong!");
            }

            var Token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();

            SetRefreshToken(refreshToken);

            await _userService.SetTokenToUserAsync(user.Username,refreshToken);

            return Ok(Token);
        }



        #endregion

        #region RefreshToken

        [HttpPost("refresh-token"),Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var valid = await CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            var userName = _userService.GetMyName();

            var user = await _userService.GetUserAsync(userName);

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            _userService.SetTokenToUserAsync(userName,newRefreshToken);

            return Ok(token);
        }

        #endregion

        #region Methods

        private async Task<ActionResult?> CheckTokenValidation()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userName = _userService.GetMyName();
            if (userName.IsNullOrEmpty() || refreshToken.IsNullOrEmpty())
            {
                return BadRequest("Username or RefreshToken is Empty!");
            }
            var user = await _userService.GetUserAsync(userName);

            if (user == null)
            {
                return BadRequest("NoSuchAUser!");
            }

            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            return null;
        }
        

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddMinutes(RefreshTokenExpirationTimeInMinutes),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);


        }
        private string CreateToken(User user)
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role == 0 ? "User" : "Admin") //have to get rid of this and user.Role should return string from enum
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value
                ));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(JWTExpirationTimeInMinutes),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        #endregion
    }
}
