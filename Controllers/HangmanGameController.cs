using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Hangman.BusinessLogics;
using Hangman.Data;
using Hangman.Extensions;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Services.SessionService;
using Hangman.Services.WordService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Controllers
{
    [Route("/api/HangmanGameApi")]
    [ApiController]
    public class HangmanGameController : Controller
    {
        #region Constructor

        private readonly IUserService _userService;
        private readonly IWordService _wordService;
        private readonly ISessionService _sessionService;
        private readonly IGuessBusinessLogic _guessBusinessLogic;

        public HangmanGameController(  IUserService userService,IWordService wordService, ISessionService sessionService, IGuessBusinessLogic guessBusinessLogic)
        {
            _userService = userService;
            _wordService = wordService;
            _sessionService = sessionService;
            _guessBusinessLogic = guessBusinessLogic;
        }
        #endregion

        #region Guess

        [HttpPost("Guess")]
        public async Task<ActionResult<GuessResponseModel>> Guess(GuessRequestModel request)
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            request.Guess = request.Guess.ToLower();

            var username = _userService.GetMyName();

            var session = _sessionService.GetSession(username,request.GameId);

            if (session == null)
            {
                return BadRequest("No Such A Session!");
            }
            if (session.IsEnded)
            {
                return BadRequest("This Session is Already Ended!");
            }
            if (request.Guess.IsNullOrEmpty()|| string.IsNullOrWhiteSpace(request.Guess) || (!request.Guess.All(Char.IsLetter) && !request.IsWordGuess) || (!request.IsWordGuess && request.Guess.Length > 1))//TODO: check what happens when input is not a string or char
            {
                return BadRequest("Invalid Input!");
            }

            var response = _guessBusinessLogic.GuessBL(request,session);

            return Ok(response);
            
        }
        #endregion

        #region GetSessions

        [HttpGet("Sessions")]
        public async Task<ActionResult<List<GetSessionsResponseDto>>> GetSessions()
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            var username = _userService.GetMyName();

            

            var SessionList = _sessionService.GetAllActiveSessions(username);

            

            return SessionList.Any() ? Ok(SessionList) : NoContent();

        }

        #endregion

        #region StartGame

        [HttpPost("StartNewGame")]
        public async Task<ActionResult<NewGameResponseDto>> StartNewGame(NewGameRequestDto request)
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }
            if (request.Difficulty < 1 || request.Difficulty > 3)
            {
                return BadRequest("Difficulty Should Be In Between 1 and 3!");
            }

            var RandomWord = _wordService.GetRandomWordWithGivenDifficulty(request.Difficulty);
            var UserName = _userService.GetMyName().ToLower();

            var Response = _sessionService.NewGame(RandomWord,UserName);


            return Ok(Response);
        }


        #endregion

        #region AddWord

        [HttpPost("AddWord")]
        public ActionResult<bool> AddWord(List<string> words)
        {

            _wordService.AddWords(words);


            return Ok(true);


        }




        #endregion

        #region Methods
        private ActionResult? CheckTokenValidation()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userName = _userService.GetMyName();
            if (userName.IsNullOrEmpty())
            {
                return Unauthorized("Invalid Token or Token Expired!");
            }
            
            var user = _userService.GetUser(userName);

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
