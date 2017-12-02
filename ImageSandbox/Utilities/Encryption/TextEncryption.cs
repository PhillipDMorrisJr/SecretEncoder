using System.Text;

namespace ImageSandbox.Utilities.Encryption
{
    /// <summary>
    ///     Encrypts text into an image.
    /// </summary>
    public class TextEncryption
    {
        #region Methods

        /// <summary>
        ///     Encrypts the decrypt text.
        /// </summary>
        /// <param name="textToCrypt">The text to crypt.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns></returns>
        public string EncryptDecryptText(string textToCrypt, int rotation)
        {
            var result = new StringBuilder();

            foreach (var currentChar in textToCrypt)
                if (currentChar >= 'a' && currentChar <= 'm' || currentChar >= 'A' && currentChar <= 'M')
                    result.Append((char) (currentChar + rotation));
                else if (currentChar >= 'n' && currentChar <= 'z' || currentChar >= 'N' && currentChar <= 'Z')
                    result.Append((char) (currentChar - rotation));
                else
                    result.Append(currentChar);

            return result.ToString();
        }

        /// <summary>
        ///     Adds the end of message sequence marker.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns></returns>
        public string AddEndOfMessageSequence(string cipherText)
        {
            var result = new StringBuilder();

            result.Append("!EOM!");

            return result.ToString();
        }

        #endregion
    }
}