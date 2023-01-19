using Hangman.Models.RequestModels;
using Hangman.Models.ResponseModels;
using Hangman.Models;

namespace Hangman.BusinessLogics
{
    public interface IGuessBusinessLogic
    {
        public GuessResponseModel GuessBL(GuessRequestModel request, Session session);
    }
}
