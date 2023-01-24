using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException(string username) 
            : base($"User with <{username}> username does not exists.")
        {
        }
    }
}
