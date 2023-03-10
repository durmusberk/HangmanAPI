using Azure.Core;
using Hangman.Data;
using Hangman.Extensions;
using Hangman.Models;
using Hangman.Models.Exceptions;
using Hangman.Models.ResponseModels;

namespace Hangman.Services.SessionService
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _db;
        private readonly UnitOfWork _unitOfWork = new();

        public SessionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public void DeleteSessionsOfUser(string username)
        {
            var all_sessions = _unitOfWork.SessionRepository.Get(x => x.Username == username).ToList();
            if (all_sessions.Any())
            {
                _unitOfWork.SessionRepository.Delete(all_sessions);
                _unitOfWork.SaveAsync();
            }

        }

        public void EndSession(Session session, bool IsGuessed)
        {
            session.IsGuessed = IsGuessed;
            session.IsEnded = true;
            session.DateEnded = DateTime.UtcNow;
            if (IsGuessed)
            {
                session.GuessedChars = session.GuessedChars.Replace('0', '1');
            }

            _unitOfWork.SessionRepository.Update(session);
            _unitOfWork.SaveAsync();
        }

        public List<GetSessionsResponseDto> GetAllActiveSessions(string username)
        {
            var active_session_list =  _unitOfWork.SessionRepository.Get(x => x.Username == username && !x.IsEnded).ToList().Select(x => new GetSessionsResponseDto
            {
                GameId = x.GameId,
                DashedWord = HelperExtension.CreateDashedWord(x.Word, HelperExtension.CSVToIntArray(x.GuessedChars)),
                WrongGuessCount = x.WrongGuessCount,
                RemainingGuessCount = x.Word.Length - x.WrongGuessCount - x.Word.Count(Char.IsWhiteSpace),
                Difficulty = x.Difficulty
            }
            ).ToList(); ; //hata verirse selectten once tolist yap veya createdashed wordu kendin yaz

            return active_session_list;
        }

        public List<GetSessionsResponseDto> GetAllSessions(string username)
        {
            var session_list = _unitOfWork.SessionRepository.Get(x => x.Username == username).ToList().Select(x => new GetSessionsResponseDto
            {
                GameId = x.GameId,
                DashedWord = HelperExtension.CreateDashedWord(x.Word, HelperExtension.CSVToIntArray(x.GuessedChars)),
                WrongGuessCount = x.WrongGuessCount,
                RemainingGuessCount = x.Word.Length - x.WrongGuessCount - x.Word.Count(Char.IsWhiteSpace),
                Difficulty = x.Difficulty
            }
            ).ToList(); //hata verirse selectten once tolist yap veya createdashed wordu kendin yaz

            return session_list;
        }



        public int GetLastGameId(string username)
        {
            
            var LastSession = _unitOfWork.SessionRepository.Get(s => s.Username == username).OrderByDescending(u => u.GameId).FirstOrDefault();
            int LastGameId = 0;
            if (LastSession != null)
            {
                LastGameId = LastSession.GameId;
            }

            return LastGameId;
        }

        public Session? GetSession(string username, int GameId)
        {
           return _unitOfWork.SessionRepository.Get(u => u.Username == username && GameId == u.GameId).FirstOrDefault();
        }

        public Session GetSessionForGuess(string username, int GameId)
        {
            var session = GetSession(username, GameId);

            if (session == null)
            {
                throw new SessionNotFoundException(GameId);
            }
            if (session.IsEnded)
            {
                throw new SessionAlreadyEndedException(GameId);
            }
            return session;
        }

        public NewGameResponseDto NewGame(Word word,string username)
        {
            var WordLowerCase = word.Name.ToLower();
            int WordCount = WordLowerCase.Split(" ").Length;
            var GuessedCharsCSV = HelperExtension.IntArrayToCSV(WordLowerCase.Length);
            int LastGameId = GetLastGameId(username) + 1;

            var new_session = new Session()
            {
                Username = username,
                Word = WordLowerCase,
                IsEnded = false,
                IsGuessed = false,
                WrongGuessCount = 0,
                DateStarted = DateTime.Now,
                GuessedChars = GuessedCharsCSV,
                GameId = LastGameId,
                Difficulty = word.Difficulty
            };

            _unitOfWork.SessionRepository.InsertAsync(new_session);
            _unitOfWork.SaveAsync();
            
            var response = new NewGameResponseDto()
            {
                GameId = LastGameId,
                WordCount = WordCount,
                Difficulty = word.Difficulty,
                DashedWord = HelperExtension.CreateDashedWord(WordLowerCase, GuessedCharsCSV),
                DateStarted = new_session.DateStarted

            };

            return response;
        }

        
    }
}
