using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Utilities.Embedder
{
    /// <summary>
    /// Encrypts an image into an image.
    /// </summary>
    public static class ImageEmbedder
    {
        /// <summary>
        /// Embeds the image with image.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="embedPixels">The embed pixels.</param>
        /// <param name="embedImageWidth">Width of the embed image.</param>
        /// <param name="embedImageHeight">Height of the embed image.</param>
        /// <param name="sourceImageWidth">Width of the source image.</param>
        /// <param name="sourceImageHeight">Height of the source image.</param>
        public static void EmbedImageWithImage(byte[] sourcePixels, byte[] embedPixels, uint embedImageWidth, uint embedImageHeight, uint sourceImageWidth, uint sourceImageHeight)
        {
            Color sourceColor = Color.FromArgb(119, 119, 119, 119);

            if (embedImageWidth > sourceImageWidth || embedImageHeight > sourceImageHeight)
            {
                return;
            }

            for (int y = 0; y < embedImageHeight; y++)
            {
                for (int x = 0; x < embedImageWidth; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        setPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                    else if (x == 1 && y == 0)
                    {
                        sourceColor = getPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.R |= (0 << 0);
                        setPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                    else
                    {
                        var embedColor = getPixelBgra8(embedPixels, x, y, embedImageWidth, embedImageHeight);
                        if (embedColor.Equals(Colors.Black))
                        {
                            sourceColor = getPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (0 << 0);
                            setPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                        else
                        {
                            sourceColor = getPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (1 << 0);
                            setPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                    }
                }
            }
        }

        private static Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            if (offset + 2 >= pixels.Length)
            {
                return Colors.Black;
            }
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
            
           
            
        }

        private static void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {

                var offset = (x * (int) width + y) * 4;
            if (offset + 2 >= pixels.Length)
            {
                return;
            }
            pixels[offset + 2] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 0] = color.B;
            

        }

        /// <summary>
        /// Extracts the image with image.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="sourceImageWidth">Width of the source image.</param>
        /// <param name="sourceImageHeight">Height of the source image.</param>
        /// <returns></returns>
        public static byte[] ExtractImageWithImage(byte[] sourcePixels, uint sourceImageWidth, uint sourceImageHeight)
        {
            byte[] imageExtract = new byte[sourcePixels.Length];


            for (var i = 0; i < sourceImageHeight; i++)
            {
                for (var j = 0; j < sourceImageWidth; j++)
                {
                    Color sourceColor;
                    switch (i)
                    {
                        case 0 when j == 0:
                            sourceColor = getPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);

                            break;
                        case 1 when j == 0:
                            int bitVal0 = sourceColor.R & 1;
                            if (bitVal0 == 1)
                            {
                                // throw new ArgumentException("This file should use text extraction");
                                //TODO: Call text Extraction

                            }
                            break;
                        default:

                            sourceColor = getPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);

                            int bitVal = sourceColor.B & 1;
                            if (bitVal == 0)
                            {
                                setPixelBgra8(imageExtract, i, j, Colors.Black, sourceImageWidth, sourceImageHeight);
                            }
                            else
                            {
                                setPixelBgra8(imageExtract, i, j, Colors.White, sourceImageWidth, sourceImageHeight);
                            }

                            break;
                    }
                }
            }

            return imageExtract;

        }

    }
}