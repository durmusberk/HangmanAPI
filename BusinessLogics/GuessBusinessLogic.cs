using Hangman.Data;
using Hangman.Extensions;
using Hangman.Models;
using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Services.SessionService;

namespace Hangman.BusinessLogics
{
    public class GuessBusinessLogic : IGuessBusinessLogic
    {
        private readonly ISessionService _sessionService;
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public GuessBusinessLogic(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public GuessResponseModel GuessBL(GuessRequestDto request,Session session)
        {

            request.Guess = request.Guess.ToLower();

            var IsCorrect = IsGuessCorrect(request, session);

            GuessResponseModel response;

            if (IsCorrect && request.IsWordGuess)
            {
                _sessionService.EndSession(session, IsGuessed: true);

                response = new GuessResponseModel()
                {
                    GameId = request.GameId,
                    DashedWord = request.Guess,
                    IsCorrect = IsCorrect,
                    IsFinished = true,
                    WrongGuessCount = session.WrongGuessCount,
                    RemainingGuessCount = session.Word.Length - session.WrongGuessCount - session.Word.Count(Char.IsWhiteSpace),
                    Message = "Congratulations! Your Word Guess is Correct!"
                };
            }

            else if (IsCorrect && !request.IsWordGuess)
            {
                var DashedWord = HelperExtension.CreateDashedWord(session.Word, session.GuessedChars);

                if (DashedWord == session.Word)
                {
                    _sessionService.EndSession(session, IsGuessed: true);
                }

                var messageEnd = session.IsEnded ? "Congratulations! You Found The Word! " : "Continue to Guess!";

                response = new GuessResponseModel()
                {
                    GameId = request.GameId,
                    DashedWord = DashedWord,
                    IsCorrect = IsCorrect,
                    IsFinished = session.IsEnded,
                    WrongGuessCount = session.WrongGuessCount,
                    RemainingGuessCount = session.Word.Length - session.WrongGuessCount - session.Word.Count(Char.IsWhiteSpace),
                    Message = "Your Letter Guess is Correct. " + messageEnd

                };
            }
            //not correct
            else
            {
                session.WrongGuessCount++;
                if (IsGameOver(session))
                {
                    _sessionService.EndSession(session, IsGuessed: false);
                    response = new GuessResponseModel()
                    {
                        GameId = session.GameId,
                        DashedWord = session.Word,
                        IsCorrect = IsCorrect,
                        IsFinished = true,
                        WrongGuessCount = session.WrongGuessCount,
                        RemainingGuessCount = 0,
                        Message = "Oops! You have failed. Your Word Was [ " + session.Word + " ]. Let's Start A New Game And Try Again."
                    };

                }
                else
                {
                    response = new GuessResponseModel()
                    {
                        GameId = session.GameId,
                        DashedWord = HelperExtension.CreateDashedWord(session.Word, session.GuessedChars),
                        IsCorrect = IsCorrect,
                        IsFinished = false,
                        WrongGuessCount = session.WrongGuessCount,
                        RemainingGuessCount = session.Word.Length - session.WrongGuessCount - session.Word.Count(Char.IsWhiteSpace),
                        Message = "Oh! Your Guess is Incorrect. Try Again!"
                    };
                }
                _unitOfWork.SessionRepository.Update(session);


            }

            _unitOfWork.SaveAsync();
            return response;
        }
        private bool IsGameOver(Session session)
        {
            return session.WrongGuessCount >= session.Word.Length - session.Word.Count(Char.IsWhiteSpace);
        }
        private bool IsGuessCorrect(GuessRequestDto request, Session session)
        {
            if (request.IsWordGuess)
            {
                return request.Guess == session.Word;
            }


            int[] GuessedChars = HelperExtension.CSVToIntArray(session.GuessedChars);

            int GuessCount = 0;

            for (int i = 0; i < session.Word.Length; i++)
            {
                if (GuessedChars[i] == 0 && session.Word[i] != ' ' && session.Word[i].ToString() == request.Guess)
                {
                    GuessedChars[i] = 1;
                    GuessCount++;
                }
            }

            session.GuessedChars = HelperExtension.IntArrayToCSV(GuessedChars);
            _unitOfWork.SessionRepository.Update(session);
            _unitOfWork.SaveAsync();


            return GuessCount > 0;

        }
    }
}
