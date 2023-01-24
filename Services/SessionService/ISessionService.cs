using Hangman.Models;
using Hangman.Models.ResponseModels;

namespace Hangman.Services.SessionService
{
    public interface ISessionService
    {
        NewGameResponseDto NewGame(Word word,string username);
        int GetLastGameId(string username);
        List<GetSessionsResponseDto> GetAllActiveSessions(string username);
        Session? GetSession(string username,int GameId);
        void EndSession(Session session, bool IsGuessed);
        void DeleteSessionsOfUser(string username);

        Session GetSessionForGuess(string username,int gameId);
    }
}
