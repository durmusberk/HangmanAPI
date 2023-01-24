namespace Hangman.Models.Exceptions
{
    public class WrongPasswordException : Exception
    {
        //public static readonly int statusCode = StatusCodes.Status400BadRequest;
        public WrongPasswordException() : base($"Your Password is Wrong!")
        {

        }
    }
}
