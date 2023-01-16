using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Azure.Core;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Controllers
{



    [Route("/api/HangmanAPI")]
    [ApiController]
    public class HangmanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public HangmanController(ApplicationDbContext db, IMapper mapper,IConfiguration configuration)
        {
            _db= db;
            _mapper= mapper;
            _configuration = configuration;
        }

        #region GetAllUsers

        [HttpGet("Users")]
        public  ActionResult<List<User>> GetUsers()
        {
            var list =  _db.Users.ToList();

            return Ok(list);
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


            return  Ok(Token);




        }

        private string CreateToken(User user)
        {

            List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value
                ));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddSeconds(60),
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



        #region Methods
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        

        #endregion
    }
}
