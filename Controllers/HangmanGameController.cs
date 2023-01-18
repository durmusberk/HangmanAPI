using AutoMapper;
using Hangman.Data;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Hangman.Controllers
{
    public class HangmanGameController : Controller
    {
        #region Constructor

        
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userServices;
        public HangmanGameController(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, IUserService userServices)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _userServices = userServices;
        }
        #endregion

        #region StartGame

        [HttpPost("StartNewGame")]
        public async Task<ActionResult<NewGameResponseDto>> StartNewGame(NewGameRequestDto request)
        {
            return Ok();
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
        #endregion
    }
}
