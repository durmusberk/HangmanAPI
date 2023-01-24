using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class UserAlreadyExistsException : AlreadyExistsException
    {
        public UserAlreadyExistsException(string username) 
            : base($"The User with the {username} username is already exists. ")
        {

        }

    }
}
