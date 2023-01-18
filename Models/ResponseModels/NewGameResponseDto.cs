using System;

namespace Hangman.Models.ResponseModels
{
    public class NewGameResponseDto
    {
        public int GameId { get; set; }
        public int Difficulty { get; set; }
        public int WordCount { get; set; }
        public string DashedWord { get; set; }
        public DateTime DateStarted { get; set; } = DateTime.Now;

    }
}