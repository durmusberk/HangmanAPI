namespace Hangman.Models.ResponseModels
{
    public class UserLoginResponseDto
    {
        public string Username { get; set; }

        public string Token { get; set; }

        public string TimeToLive { get; set; }
    }
}
