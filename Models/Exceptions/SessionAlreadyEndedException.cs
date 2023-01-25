using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class SessionAlreadyEndedException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public SessionAlreadyEndedException(int id)
        {
            StatusCode = StatusCodes.Status303SeeOther;
            Message = $"Session with {id} Game Id is Already Ended!";
        }
    }
}
