using System;

namespace Hangman.Models.ResponseModels
{
    public class NewGameResponseDto
    {
        public int SessionId { get; set; }
        public int Difficulty { get; set; }
        public int WordCount { get; set; }
        public int LetterCount { get; set; }
        public DateTime DateStarted { get; set; } = DateTime.Now;

    }
}