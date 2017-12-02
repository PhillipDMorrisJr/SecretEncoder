using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSandbox.Utilities.Embedder
{
    /// <summary>
    /// Embeds a text message into an image.
    /// </summary>
    public class TextEmbedder
    {

        /// <summary>
        /// Converts the character to ASCII decimal value.
        /// </summary>
        /// <param name="charToConvert">The character to convert.</param>
        /// <returns></returns>
        public int ConvertCharToAsciiDecimal(char charToConvert)
        {
            int result = System.Convert.ToInt32(charToConvert);

            return result;
        }

        /// <summary>
        /// Converts the text to ASCII decimal.
        /// </summary>
        /// <param name="textToConvert">The text to convert.</param>
        /// <returns></returns>
        public List<int> ConvertTextToAsciiDecimal(string textToConvert)
        {
            List<int> result = new List<int>();

            foreach (char currentChar in textToConvert)
            {
                result.Add(this.ConvertCharToAsciiDecimal(currentChar));
            }

            return result;
        }

        /// <summary>
        /// Converts the ASCII decimal to binary.
        /// </summary>
        /// <param name="decimalToConvert">The decimal to convert.</param>
        /// <returns></returns>
        public int ConvertAsciiDecimalToBinary(int decimalToConvert)
        {
            string paddedBinary = Convert.ToString(Convert.ToByte(decimalToConvert), 2).PadLeft(8, '0');
            
            int result = Int32.Parse(paddedBinary);

            return result;
        }

        /// <summary>
        /// Converts ASCII text to binary.
        /// </summary>
        /// <param name="textToConvert">The text to convert.</param>
        /// <returns></returns>
        public string ConvertAsciiToBinary(string textToConvert)
        {
            string result = string.Empty;

            byte[] textBytes = Encoding.ASCII.GetBytes(textToConvert);

            foreach (byte currentByte in textBytes)
            {
                result += Convert.ToString(currentByte, 2).PadLeft(8, '0');
            }

            return result;
        }
    }
}
