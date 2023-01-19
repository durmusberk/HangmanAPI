using System.Text;

namespace Hangman.Extensions
{
    public static class HelperExtension
    {
        /// <summary>
        /// A Method that takes a strin and int array, and returns the diguised version of the string according to the int array
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guessedChars"></param>
        /// <returns>string</returns>
        public static string CreateDashedWord(string name, int[] guessedChars)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < guessedChars.Length; i++)
            {
                if (guessedChars[i] == 0 && name[i] != ' ')
                {
                    stringBuilder.Append('_');
                }
                else
                {
                    stringBuilder.Append(name[i]);
                }
            }

            return stringBuilder.ToString();
        }

        public static string CreateDashedWord(string name, string guessedCharsCSV)
        {
            var guessedChars = CSVToIntArray(guessedCharsCSV);
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < guessedChars.Length; i++)
            {
                if (guessedChars[i] == 0 && name[i] != ' ')
                {
                    stringBuilder.Append('_');
                }
                else
                {
                    stringBuilder.Append(name[i]);
                }
            }

            return stringBuilder.ToString();
        }

        public static int[] CSVToIntArray(string text) => Array.ConvertAll(text.Split(','), int.Parse);
        public static  string IntArrayToCSV(int[] array) => string.Join(",", array);
        public static string IntArrayToCSV(int len) => string.Join(",", new int[len]);

        public static int SetDifficulty(string item)
        {
            if (item.Length > 10)
            {
                return 3;
            }
            else if (item.Length > 5)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }
}
