using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class WrongPasswordException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }

        public WrongPasswordException()
        {
            StatusCode = StatusCodes.Status400BadRequest;
            Message = "Your Password is Wrong!";
        }

    }
}
