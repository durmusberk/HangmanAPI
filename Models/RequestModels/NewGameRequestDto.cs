using FluentValidation;

namespace Hangman.Models.RequestModels
{
    public class NewGameRequestDto
    {
        public int Difficulty { get; set; }


        public class NewGameRequestValidator : AbstractValidator<NewGameRequestDto>
        {
            
            public NewGameRequestValidator()
            {
                RuleFor(x => x.Difficulty).NotEmpty().InclusiveBetween(1, 3);
            }
        }
    }
}
