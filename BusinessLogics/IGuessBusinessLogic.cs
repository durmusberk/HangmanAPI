using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Models;

namespace Hangman.BusinessLogics
{
    public interface IGuessBusinessLogic
    {
        public GuessResponseModel GuessBL(GuessRequestDto request, Session session);
    }
}
