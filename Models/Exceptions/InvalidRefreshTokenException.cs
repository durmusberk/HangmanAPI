namespace Hangman.Models.Exceptions
{
    public class InvalidRefreshTokenException : Exception
    {
        public InvalidRefreshTokenException() : base("Your Token is Invalid. Please Login Again!") 
        { }
    }
}
