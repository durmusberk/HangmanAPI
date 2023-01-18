using System.Text;
using AutoMapper;
using Hangman.Data;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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

        #region StartGame

        [HttpPost("StartNewGame")]
        public async Task<ActionResult<NewGameResponseDto>> StartNewGame(NewGameRequestDto request)
        {
            var valid = CheckTokenValidation();
            if (valid != null)
            {
                return valid;
            }

            var Random_Word = _db.Words.OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefault();
            int WordCount = Random_Word.Name.Split(" ").Length;
            var GuessedChars = new int[Random_Word.Name.Length];
            var GuessedCharsToCSV = string.Join(",", GuessedChars);
            var DashedWord = CreateDashedWord(Random_Word.Name, GuessedChars);

            int LastGameId;
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
                Word = Random_Word.Name,
                IsEnded = false,
                IsGuessed = false,
                WrongGuessCount = 0,
                DateStarted = DateTime.Now,
                GuessedChars = GuessedCharsToCSV,
                GameId = LastGameId + 1
            };
            
            _db.Sessions.Add(new_session);

            int rowsAffected;

            try
            {
                rowsAffected = _db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var response = new NewGameResponseDto()
            {
                GameId = new_session.GameId,
                WordCount= WordCount,
                Difficulty = Random_Word.Difficulty,
                DashedWord= DashedWord,
                DateStarted = new_session.DateStarted
                
            } ;


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

                var new_word = new Word() { Name = item,Difficulty = WordDifficulty };

                if (!_db.Words.Contains(new_word))
                {
                    _db.Words.Add(new_word);
                }

            }

            int rowsAffected;

            try
            {
                rowsAffected = _db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);   
            }


            return rowsAffected > 0 ? Ok(true) : BadRequest(false);
            

        }




        #endregion

        #region Methods
        private string CreateDashedWord(string name, int[] guessedChars)
        {
            StringBuilder stringBuilder= new StringBuilder();
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
            if (userName.IsNullOrEmpty() || refreshToken.IsNullOrEmpty())
            {
                return BadRequest("Username or RefreshToken is Empty!");
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
