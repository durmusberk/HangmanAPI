using FluentValidation.Results;

namespace Hangman.Models.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(ValidationResult ms) : base(ms.ToString()) { }
    }
}
