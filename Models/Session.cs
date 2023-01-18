using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hangman.Models
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public string Username { get; set; }
        [Required]
        public int GameId { get; set; }
        public string Word { get; set; }  
        public string GuessedChars { get; set; } = string.Empty;
        public int WrongGuessCount { get; set; } = 0;
        public bool IsEnded { get; set; }
        public bool IsGuessed { get; set; }
        [Required]
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(Id)}={Id.ToString()}, {nameof(Username)}={Username}, {nameof(Word)}={Word}, {nameof(GuessedChars)}={GuessedChars}, {nameof(WrongGuessCount)}={WrongGuessCount.ToString()}, {nameof(IsEnded)}={IsEnded.ToString()}, {nameof(IsGuessed)}={IsGuessed.ToString()}, {nameof(DateStarted)}={DateStarted.ToString()}, {nameof(DateEnded)}={DateEnded.ToString()}}}";
        }
    }   
}
