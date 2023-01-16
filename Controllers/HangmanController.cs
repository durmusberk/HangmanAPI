using System.Security.Cryptography;
using AutoMapper;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Controllers
{



    [Route("/api/HangmanAPI")]
    [ApiController]
    public class HangmanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public HangmanController(ApplicationDbContext db, IMapper mapper)
        {
            _db= db;
            _mapper= mapper;
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

            if (request == null || request.Username == null || request.Password == null)
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

        

        #region Methods
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        #endregion
    }
}
