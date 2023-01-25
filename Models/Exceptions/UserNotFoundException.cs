using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class UserNotFoundException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public UserNotFoundException(string username)
        {
            StatusCode = StatusCodes.Status404NotFound;
            Message = $"User with <{username}> username does not exists.";
        }
        public UserNotFoundException()
        {
            StatusCode = StatusCodes.Status404NotFound;
            Message = "Not Any User Found!";
        }
    }
}
