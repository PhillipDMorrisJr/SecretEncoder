using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Model.Encryption
{
    /// <summary>
    /// Encrypts an image into an image.
    /// </summary>
    public static class ImageEmbedder
    {
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
                        SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                    else if (x == 1 && y == 0)
                    {
                        sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.R |= (0 << 0);
                        SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                    else
                    {
                        var embedColor = GetPixelBgra8(embedPixels, x, y, embedImageWidth, embedImageHeight);
                        if (embedColor.Equals(Colors.Black))
                        {
                            sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (0 << 0);
                            SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                        else
                        {
                            sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (1 << 0);
                            SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                    }
                }
            }
        }

        private static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
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

        private static void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
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
                            sourceColor = GetPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);
                           
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
                            
                            sourceColor = GetPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);
                            
                            int bitVal = sourceColor.B & 1;
                            if (bitVal == 0)
                            {
                                SetPixelBgra8(imageExtract, i, j, Colors.Black, sourceImageWidth, sourceImageHeight);
                            }
                            else
                            {
                                SetPixelBgra8(imageExtract, i, j,  Colors.White, sourceImageWidth, sourceImageHeight);
                            }
                            
                            break;
                    }
                }
            }

            return imageExtract;

        }

    }
}