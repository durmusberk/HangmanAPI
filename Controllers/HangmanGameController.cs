﻿using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

namespace Hangman.Controllers
{
    [Route("/api/HangmanGameApi")]
    [ApiController]
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

        #region Guess

        [HttpPost("Guess")]
        public async Task<ActionResult<GuessResponseModel>> Guess(GuessRequestModel request)
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            var username = _userServices.GetMyName();

            var session = _db.Sessions.FirstOrDefault(u => u.Username == username && request.GameId == u.GameId);

            if (session == null)
            {
                return BadRequest("No Such A Session!");
            }
            if (session.IsEnded)
            {
                return BadRequest("This Session is Already Ended!");
            }
            if (request.Guess.IsNullOrEmpty()|| string.IsNullOrWhiteSpace(request.Guess) || !request.Guess.All(Char.IsLetter) || (!request.IsWordGuess && request.Guess.Length > 1))//TODO: check what happens when input is not a string or char
            {
                return BadRequest("Invalid Input!");
            }

            request.Guess =request.Guess.ToLower();
            

            var IsCorrect = IsGuessCorrect(request, session);

            GuessResponseModel response;

            if (IsCorrect && request.IsWordGuess) 
            { 
                session.IsEnded= true;
                session.DateEnded = DateTime.UtcNow;
                session.IsGuessed = true;
                session.GuessedChars = session.GuessedChars.Replace('0', '1');
                

                response = new GuessResponseModel()
                {
                    DashedWord = request.Guess,
                    GameId = request.GameId,
                    IsCorrect = IsCorrect,
                    IsFinished = true,
                    RemainingGuessCount = session.Word.Length -session.WrongGuessCount- session.Word.Count(Char.IsWhiteSpace),
                    WrongGuessCount = session.WrongGuessCount
                };

                
                
            }

            if (IsCorrect && !request.IsWordGuess)
            {
                var DashedWord = CreateDashedWord(session.Word, CSVToIntArray(session.GuessedChars));
                response = new GuessResponseModel()
                {
                    DashedWord = DashedWord,
                    GameId = request.GameId,
                    IsCorrect = IsCorrect,
                    IsFinished = DashedWord == session.Word,
                    WrongGuessCount = session.WrongGuessCount,
                    RemainingGuessCount = session.Word.Length - session.WrongGuessCount - session.Word.Count(Char.IsWhiteSpace)

                };
                if (response.IsFinished) 
                {
                    session.IsEnded = true;
                    session.DateEnded = DateTime.UtcNow;
                    session.IsGuessed = true;
                    session.GuessedChars = session.GuessedChars.Replace('0', '1');
                }
                
                
            }

            //not correct
            session.WrongGuessCount++;
            if (IsGameOver(session))
            {
                response = new GuessResponseModel()
                {
                    DashedWord = session.Word,
                    GameId = session.GameId,
                    IsCorrect = IsCorrect,
                    IsFinished = true,
                    RemainingGuessCount = 0,
                    WrongGuessCount = session.WrongGuessCount                    
                };
                
            }

            response = new GuessResponseModel()
            {
                GameId= session.GameId,
                DashedWord = CreateDashedWord(session.Word, CSVToIntArray(session.GuessedChars)),
                IsCorrect = IsCorrect,
                IsFinished = false,
                WrongGuessCount= session.WrongGuessCount,
                RemainingGuessCount = session.Word.Length - session.WrongGuessCount - session.Word.Count(Char.IsWhiteSpace)
            };
            
            _db.SaveChanges();
            return response;
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

            var username = _userServices.GetMyName();

            var session_list = _db.Sessions.Where(u => u.Username == username && !u.IsEnded).Select(x => new GetSessionsResponseDto
            {
                GameId = x.GameId,
                DashedWord = CreateDashedWord(x.Word, CSVToIntArray(x.GuessedChars)),
                WrongGuessCount = x.WrongGuessCount,
                RemainingGuessCount = x.Word.Length - x.WrongGuessCount - x.Word.Count(Char.IsWhiteSpace),
                Difficulty = x.Difficulty
            }).ToList(); //hata verirse selectten once tolist yap veya createdashed wordu kendin yaz

            

            return session_list.Any() ? Ok(session_list) : NoContent();

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
            //_wordService.GetRandomWord();
            var Random_Word = _db.Words.OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefault();
            var word = Random_Word.Name.ToLower();
            var difficulty = Random_Word.Difficulty;
            int WordCount = word.Split(" ").Length;
            var GuessedChars = new int[word.Length];
            var GuessedCharsToCSV = IntArrayToCSV(GuessedChars);
            var DashedWord = CreateDashedWord(word, GuessedChars);
            //_wordService.GetRandomWord() return word;
            int LastGameId;

            //_sessionService.NewSession(word);
            try
            {
                LastGameId = _db.Sessions.Where(s => s.Username == _userServices.GetMyName()).Max(u => u.GameId);
            }
            catch (InvalidOperationException)
            {
                LastGameId = 0;
            }

            var new_session = new Session()
            {
                Username = _userServices.GetMyName(),
                Word = word,
                IsEnded = false,
                IsGuessed = false,
                WrongGuessCount = 0,
                DateStarted = DateTime.Now,
                GuessedChars = GuessedCharsToCSV,
                GameId = LastGameId + 1,
                Difficulty = Random_Word.Difficulty
            };

            _db.Sessions.Add(new_session);

            int rowsAffected;

            rowsAffected = _db.SaveChanges();
            //_sessionService.NewSession(word) return true or return Session.Id or NewGameResponseDto;
            try
            {
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var response = new NewGameResponseDto()
            {
                GameId = new_session.GameId,
                WordCount = WordCount,
                Difficulty = Random_Word.Difficulty,
                DashedWord = DashedWord,
                DateStarted = new_session.DateStarted

            };


            return rowsAffected > 0 ? Ok(response) : BadRequest("Something Bad Happened!");
        }


        #endregion

        #region AddWord

        [HttpPost("AddWord")]
        public ActionResult<bool> AddWord(List<string> words)
        {

            foreach (var item in words)
            {
                if (item == null)
                {
                    continue;
                }

                var WordDifficulty = SetDifficulty(item);

                var new_word = new Word() { Name = item, Difficulty = WordDifficulty };

                if (!_db.Words.Contains(new_word))
                {
                    _db.Words.Add(new_word);
                }

            }

            _db.SaveChanges();


            return Ok(true);


        }




        #endregion

        #region Methods

        private bool IsGameOver(Session session)
        {
            if (session.WrongGuessCount  >= session.Word.Length - session.Word.Count(Char.IsWhiteSpace))
            {
                session.IsGuessed = false;
                session.IsEnded = true;
                session.DateEnded = DateTime.UtcNow;
                return true;
            }
            return false;

        }
        private bool IsGuessCorrect(GuessRequestModel request, Session session)
        {
            if (request.IsWordGuess)
            {
                return request.Guess == session.Word;
            }
            
            
            int[] GuessedChars = CSVToIntArray(session.GuessedChars);
            
            int GuessCount = 0;

            for (int i = 0; i < session.Word.Length; i++)
            {
                if (GuessedChars[i] == 0 && session.Word[i] != ' ' && session.Word[i].ToString() == request.Guess )
                {
                    GuessedChars[i] = 1;
                    GuessCount++;
                }
            }

            session.GuessedChars = IntArrayToCSV(GuessedChars);

            return GuessCount > 0;
            
        }

        private int[] CSVToIntArray(string text) => Array.ConvertAll(text.Split(','), int.Parse);
        private string IntArrayToCSV(int[] array) => string.Join(",", array);
        private string CreateDashedWord(string name, int[] guessedChars)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < guessedChars.Length; i++)
            {
                if (guessedChars[i] == 0 && name[i] != ' ')
                {
                    stringBuilder.Append('_');
                }
                else
                {
                    stringBuilder.Append(name[i]);
                }
            }

            return stringBuilder.ToString();
        }
        private int SetDifficulty(string item)
        {
            if (item.Length > 12)
            {
                return 3;
            }
            else if (item.Length > 6)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        private ActionResult? CheckTokenValidation()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userName = _userServices.GetMyName();
            if (userName.IsNullOrEmpty())
            {
                return Unauthorized("Invalid Token or Token Expired!");
            }
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
