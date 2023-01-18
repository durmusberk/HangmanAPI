namespace Hangman.Models.ResponseModels
{
    public class GetSessionsResponseDto
    {
        public int GameId { get; set; }
        public string DashedWord { get; set; }
        public int WrongGuessCount { get; set; }
        public int RemainingGuessCount { get; set; }

    }
}
