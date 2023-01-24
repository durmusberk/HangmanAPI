using System;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Hangman.BusinessLogics;
using Hangman.Extensions;
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
        private readonly IValidator<GuessRequestDto> _guessRequestValidator;
        private readonly IValidator<NewGameRequestDto> _newGameValidator;
        private readonly IAuthManager _authManager;

        public HangmanGameController(  IUserService userService,IWordService wordService, ISessionService sessionService, 
            IGuessBusinessLogic guessBusinessLogic, IValidator<GuessRequestDto> guessRequestValidator, IValidator<NewGameRequestDto> newGameValidator,
            IAuthManager authManager)
        {
            _userService = userService;
            _wordService = wordService;
            _sessionService = sessionService;
            _guessBusinessLogic = guessBusinessLogic;
            _guessRequestValidator = guessRequestValidator;
            _newGameValidator = newGameValidator;
            _authManager = authManager;
        }
        #endregion

        #region Guess

        [HttpPost("Guess")]
        public async Task<ActionResult<GuessResponseModel>> Guess(GuessRequestDto request)
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

            ValidationResult result = await _guessRequestValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }

            var username = _userService.GetMyName();

            var session = _sessionService.GetSession(username,request.GameId);

            if (session == null)
            {
                return NotFound("No Such A Session!");
            }
            if (session.IsEnded)
            {
                return BadRequest("This Session is Already Ended!");
            }

            var response = _guessBusinessLogic.GuessBL(request,session);

            return Ok(response);
            
        }
        #endregion

        #region GetSessions

        [HttpGet("Sessions")]
        public async Task<ActionResult<List<GetSessionsResponseDto>>> GetSessions()
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

            var username = _userService.GetMyName();

            var SessionList = _sessionService.GetAllActiveSessions(username);

            

            return SessionList.Any() ? Ok(SessionList) : NoContent();

        }

        #endregion

        #region StartGame

        [HttpPost("StartNewGame")]
        public async Task<ActionResult<NewGameResponseDto>> StartNewGame(NewGameRequestDto request)
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

            ValidationResult result = await _newGameValidator.ValidateAsync(request);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
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

        
    }
}
