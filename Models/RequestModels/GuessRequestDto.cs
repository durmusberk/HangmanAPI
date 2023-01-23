using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace Hangman.Models.RequestModels
{
    public class GuessRequestDto
    {
        public int GameId { get; set; }

        public bool IsWordGuess { get; set; }

        public string Guess { get; set; }
    }
    public class GuessRequestValidator : AbstractValidator<GuessRequestDto>
    {
        readonly Regex regExWSpace = new Regex("^[a-zA-Z ]*$");//* yerine + denenecek
        readonly Regex regEx = new Regex("^[a-zA-Z]*$");
        public GuessRequestValidator()
        {
            RuleFor(x => x.GameId).NotNull().GreaterThan(0);
            RuleFor(x => x.IsWordGuess);
            RuleFor(x => x.Guess).MinimumLength(1).MaximumLength(1).NotNull().NotEmpty().Matches(regEx).When(x => !x.IsWordGuess);
            RuleFor(x => x.Guess).MinimumLength(2).NotNull().NotEmpty().Matches(regExWSpace).When(x => x.IsWordGuess); 
            
        }
    }
}