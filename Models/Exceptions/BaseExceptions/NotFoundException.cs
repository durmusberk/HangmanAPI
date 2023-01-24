namespace Hangman.Models.Exceptions.BaseExceptions
{
    public class NotFoundException : Exception
    {
        protected NotFoundException(string message) : base(message)
        {

        }
    }
}
