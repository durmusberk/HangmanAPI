using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class UserAlreadyExistsException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public UserAlreadyExistsException(string username)
        {
            StatusCode = StatusCodes.Status400BadRequest;
            Message = $"The User with the {username} username is already exists. ";

        }

    }
}
