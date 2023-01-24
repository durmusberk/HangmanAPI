namespace Hangman.Models.Exceptions
{
    public class SessionAlreadyEndedException : Exception
    {
        public SessionAlreadyEndedException(int id) :base($"Session with {id} Game Id is Already Ended!") { }
    }
}
