namespace Hangman.Models.ResponseModels
{
    public class GuessResponseModel
    {
        public int GameId { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsFinished { get; set; }
        public string DashedWord { get; set; }
        public int WrongGuessCount { get; set; }
        public int RemainingGuessCount { get; set; }
        public string Message { get; set; }
    }
}