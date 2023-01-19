﻿using Azure.Core;
using Hangman.Data;
using Hangman.Extensions;
using Hangman.Models;
using Hangman.Models.ResponseModels;

namespace Hangman.Services.SessionService
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _db;

        public SessionService(ApplicationDbContext db)
        {
            _db = db;
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
        }

        public List<GetSessionsResponseDto> GetAllActiveSessions(string username)
        {
            var session_list = _db.Sessions.Where(u => u.Username == username && !u.IsEnded).ToList().Select(x => new GetSessionsResponseDto
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
            int LastGameId;

            try
            {
                LastGameId = _db.Sessions.Where(s => s.Username == username).Max(u => u.GameId);
            }
            catch (InvalidOperationException)
            {
                LastGameId = 0;
            }

            return LastGameId;
        }

        public Session? GetSession(string username, int GameId)
        {
           return _db.Sessions.FirstOrDefault(u => u.Username == username && GameId == u.GameId);
        }

        public void IncrementWrongGuessCount(Session session)
        {
            session.WrongGuessCount++;
        }

        public NewGameResponseDto NewGame(Word word,string username)
        {
            word ??= new Word { Name = "Dummy Word", Difficulty = 2};
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

            _db.Sessions.Add(new_session);
            _db.SaveChanges();


            
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

        public void SetGuessedChars(Session session, string v)
        {
            session.GuessedChars= v;
        }
    }
}
