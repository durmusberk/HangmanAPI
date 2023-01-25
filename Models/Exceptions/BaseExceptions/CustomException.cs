namespace Hangman.Models.Exceptions.BaseExceptions
{
    public abstract class CustomException : Exception
    {
        public abstract int StatusCode { get; set; }
        public abstract string Message { get; set; }
    }
}
