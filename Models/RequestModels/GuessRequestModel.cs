namespace Hangman.Models.RequestModels
{
    public class GuessRequestModel
    {
        public int GameId { get; set; }

        public bool IsWordGuess { get; set; }

        public string Guess { get; set; }
    }
}