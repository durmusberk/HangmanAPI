using FluentValidation;
using System.Text.RegularExpressions;

namespace Hangman.Models.RequestModels
{
    public class UserRegisterRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Role { get; set; }
    }

    public class UserRegisterValidator : AbstractValidator<UserRegisterRequestDto>
    {
        //Has minimum 8 characters in length.Adjust it by modifying {8,}
        //At least one uppercase English letter.You can remove this condition by removing (?=.*?[A - Z])
        //At least one lowercase English letter.You can remove this condition by removing (?=.*?[a - z])
        //At least one digit.You can remove this condition by removing (?=.*?[0 - 9])
        //At least one special character,  You can remove this condition by removing (?=.*?[#?!@$%^&*-])

        private readonly Regex regExPassword = new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$");


        private readonly Regex regExUsername = new Regex("^[a-zA-Z0-9]*$");
        public UserRegisterValidator()
        {
            RuleFor(x => x.Username).MinimumLength(4).NotNull().NotEmpty().Matches(regExUsername);
            RuleFor(x => x.Password).NotEmpty().NotNull().Matches(regExPassword).WithMessage("Your password should contain at least 1 UpperCase, 1 LowerCase, 1 Digit and 1 Special Character. It should has Min 8 Characters.");
            RuleFor(x => x.Role).InclusiveBetween(0, 1);
        }
    }
}
