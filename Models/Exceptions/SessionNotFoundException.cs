using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Models.Exceptions
{
    public sealed class SessionNotFoundException : CustomException
    {
        public override int StatusCode { get; set; }
        public override string Message { get; set; }
        public SessionNotFoundException(int id)
        {
            StatusCode = StatusCodes.Status404NotFound;
            Message = $"Session with {id} does not exists.";
        }
        public SessionNotFoundException()
        {
            StatusCode = StatusCodes.Status404NotFound;
            Message = "No Session Found!";
        }
    }
}
