namespace Hangman.Models.Exceptions
{
    public class ExpiredRefreshTokenException : Exception
    {
        public ExpiredRefreshTokenException() : base("Your Token has Expired. Please Login Again!")
        { }
    }
}
