using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class SessionNotFoundException : NotFoundException
    {
        public SessionNotFoundException(int id)
            : base($"Session with {id} does not exists.")
        {
        }
        public SessionNotFoundException()
            : base($"No Session Found!")
        {
        }
    }
}
