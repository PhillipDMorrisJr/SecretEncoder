using System.Text;
using System.Text.RegularExpressions;

namespace ImageSandbox.Utilities.Encryption
{
    /// <summary>
    ///     Encrypts text into an image.
    /// </summary>
    public class TextEncryption
    {
        #region Methods

        /// <summary>
        ///     Encrypts and decrypts the text using ROT13.
        /// </summary>
        /// <returns></returns>
        public string CryptText(string textToCrypt)
        {
            var result = new StringBuilder();
            var regex = new Regex("[A-Za-z]");

            foreach (var currentChar in textToCrypt)
            {
                if (regex.IsMatch(currentChar.ToString()))
                {
                    var charCode = ((currentChar & 223) - 52) % 26 + (currentChar & 32) + 65;
                    result.Append((char) charCode);
                }
                else
                {
                    result.Append(currentChar);
                }
            }

            return result.ToString();
        }

        #endregion
    }
}