using Hangman.Models;

namespace Hangman.Services.WordService
{
    public interface IWordService
    {
        public void AddWords(List<string> words);
        public Word GetRandomWordWithGivenDifficulty(int Difficulty);
    }
}
