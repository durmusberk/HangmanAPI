using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Controllers
{

    [Route("/api/HangmanAuthApi")]
    [ApiController]
    public class HangmanAuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userServices;
        private readonly int JWTExpirationTimeInMinutes = 30;
        private readonly int RefreshTokenExpirationTimeInMinutes = 30;


        public HangmanAuthController(ApplicationDbContext db, IMapper mapper,IConfiguration configuration, IUserService userServices)
        {
            _db= db;
            _mapper= mapper;
            _configuration = configuration;
            _userServices = userServices;
        }

        #region GetAllUsers

        [HttpGet("Users"),Authorize(Roles ="Admin")]

        public  ActionResult<List<User>> GetUsers()
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }
            

            var list =  _db.Users.ToList();

            return Ok(list);
        }


        #endregion

        #region GetMe

        [HttpGet("GetMe"), Authorize]
        public ActionResult<string> GetMe()
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }
            var userName = _userServices.GetMyName();
            return Ok(userName);
        }

        #endregion

        #region UserRegister

        [HttpPost("register")]
        public async Task<ActionResult<UserRegisterResponseDto>> Register(UserRegisterRequestDto request)
        {

            if (request == null || request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
            {
                ModelState.AddModelError("Custom Error", "Inputs Shouldn't Be Empty!");
                return BadRequest(ModelState);
            }

            if (_db.Users.FirstOrDefault(u => u.Username.ToLower() == request.Username.ToLower()) != null)
            {
                ModelState.AddModelError("Custom Error", "User Already Exists!");
                return BadRequest(ModelState);
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var new_user = new User {
                Username= request.Username,
                PasswordHash=passwordHash,
                PasswordSalt=passwordSalt,
                Role = request.Role,
                CreatedDate = DateTime.Now
            };

            _db.Users.Add(new_user);

            _db.SaveChanges();

            var response = _mapper.Map<UserRegisterResponseDto>(new_user);

            return Ok(response);
        }

        #endregion

        #region UserLogin

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDto>> Login(UserLoginRequestDto request)
        {

            if (request == null || request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
            {
                ModelState.AddModelError("Custom Error", "Inputs Shouldn't Be Empty!");
                return BadRequest(ModelState);
            }

            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == request.Username.ToLower());

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

            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;

            Console.WriteLine(user.RefreshToken + " " + user.TokenExpires + " "+user.TokenCreated);

            _db.SaveChanges();


            return  Ok(Token);
        }



        #endregion

        #region refresh-token

        [HttpPost("refresh-token"),Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            var userName = _userServices.GetMyName();

            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == userName.ToLower());

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;

            _db.SaveChanges();

            return Ok(token);
        }

        #endregion

        #region Methods

        private ActionResult? CheckTokenValidation()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userName = _userServices.GetMyName();

            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == userName.ToLower());

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
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
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
