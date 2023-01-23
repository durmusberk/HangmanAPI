using Hangman.Data;
using Hangman.Models;
using Hangman.Extensions;

namespace Hangman.Services.WordService
{
    public class WordService : IWordService
    {
        private readonly ApplicationDbContext _db;

        public WordService(ApplicationDbContext db) 
        {
            _db = db;
        }

        public void AddWords(List<string> words)
        {
            var wordlist = _db.Words.Select(u => u.Name).ToList();
            Console.WriteLine(wordlist.ToString());
            foreach (var item in words)
            {
                if (item == null)
                {
                    continue;
                }

                var WordDifficulty = HelperExtension.SetDifficulty(item);

                var new_word = new Word() { Name = item.ToLower(), Difficulty = WordDifficulty };
                

                if (!wordlist.Contains(item))
                {
                    _db.Words.Add(new_word);
                }
            }

            _db.SaveChanges();
        }

        public Word GetRandomWordWithGivenDifficulty(int Difficulty)
        {
            return _db.Words.Where(u => u.Difficulty == Difficulty).OrderBy(r => Guid.NewGuid()).Take(1).FirstOrDefault();
        }

        
    }
}
