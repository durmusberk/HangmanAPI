using Hangman.Models;
using Hangman.Models.ResponseModels;

namespace Hangman.Services.SessionService
{
    public interface ISessionService
    {
        public NewGameResponseDto NewGame(Word word,string username);
        public int GetLastGameId(string username);
        public List<GetSessionsResponseDto> GetAllActiveSessions(string username);
        public Session? GetSession(string username,int GameId);
        public void EndSession(Session session, bool IsGuessed);
        public void SetGuessedChars(Session session, string v);
        public void IncrementWrongGuessCount(Session session);
    }
}
