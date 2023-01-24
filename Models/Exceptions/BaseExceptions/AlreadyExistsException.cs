namespace Hangman.Models.Exceptions.BaseExceptions
{
    public class AlreadyExistsException : Exception
    {
        protected AlreadyExistsException(string message) : base(message)
        {

        }
    }
}
