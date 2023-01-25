using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class InvalidRefreshTokenException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public InvalidRefreshTokenException()
        {
            StatusCode = StatusCodes.Status401Unauthorized;
            Message = "Your Token is Invalid. Please Login Again!";
        }
    }


}
