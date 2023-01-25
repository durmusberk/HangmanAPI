using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class ExpiredRefreshTokenException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public ExpiredRefreshTokenException()
        {
            StatusCode = StatusCodes.Status419AuthenticationTimeout;
            Message = "Your Token has Expired. Please Login Again!";
        }

    }
}
