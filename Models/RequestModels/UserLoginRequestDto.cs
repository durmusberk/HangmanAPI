using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using FluentValidation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hangman.Models.RequestModels
{
    public class UserLoginRequestDto
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class UserLoginValidator : AbstractValidator<UserLoginRequestDto>
    {
        
        public UserLoginValidator()
        {
            RuleFor(x => x.Username).NotNull().NotEmpty();
            RuleFor(x => x.Password).NotEmpty().NotNull();
        }
    }
}
