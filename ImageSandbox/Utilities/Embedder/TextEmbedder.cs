using System;
using System.Collections.Generic;
using System.Text;

namespace ImageSandbox.Utilities.Embedder
{
    /// <summary>
    ///     Embeds a text message into an image.
    /// </summary>
    public class TextEmbedder
    {
        #region Methods

        /// <summary>
        ///     Converts the character to ASCII decimal value.
        /// </summary>
        /// <param name="charToConvert">The character to convert.</param>
        /// <returns></returns>
        public int ConvertCharToAsciiDecimal(char charToConvert)
        {
            var result = Convert.ToInt32(charToConvert);

            return result;
        }

        /// <summary>
        ///     Converts the text to ASCII decimal.
        /// </summary>
        /// <param name="textToConvert">The text to convert.</param>
        /// <returns></returns>
        public List<int> ConvertTextToAsciiDecimal(string textToConvert)
        {
            var result = new List<int>();

            foreach (var currentChar in textToConvert)
            {
                result.Add(this.ConvertCharToAsciiDecimal(currentChar));
            }

            return result;
        }

        /// <summary>
        ///     Converts the ASCII decimal to binary.
        /// </summary>
        /// <param name="decimalToConvert">The decimal to convert.</param>
        /// <returns></returns>
        public int ConvertAsciiDecimalToBinary(int decimalToConvert)
        {
            var paddedBinary = Convert.ToString(Convert.ToByte(decimalToConvert), 2).PadLeft(8, '0');

            var result = int.Parse(paddedBinary);

            return result;
        }

        /// <summary>
        ///     Converts ASCII text to binary.
        /// </summary>
        /// <param name="textToConvert">The text to convert.</param>
        /// <returns></returns>
        public string ConvertAsciiToBinary(string textToConvert)
        {
            var result = string.Empty;

            var textBytes = Encoding.ASCII.GetBytes(textToConvert);

            foreach (var currentByte in textBytes)
            {
                result += Convert.ToString(currentByte, 2).PadLeft(8, '0');
            }

            return result;
        }

        #endregion
    }
}