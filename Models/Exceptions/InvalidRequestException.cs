using FluentValidation.Results;
using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class InvalidRequestException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public InvalidRequestException(ValidationResult ms) 
        {
            StatusCode = StatusCodes.Status400BadRequest;
            Message = ms.ToString();
        }
    }
}
