using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Model.Encryption
{
    /// <summary>
    /// Encrypts an image into an image.
    /// </summary>
    public static class ImageEncryption
    {
        public static void EmbedImageWithImage(byte[] sourcePixels, byte[] embedPixels, uint embedImageWidth, uint embedImageHeight, uint sourceImageWidth, uint sourceImageHeight)
        {
            int embedXPosition = 0;
            int embedYPosition = 0;
            var embedColor = GetPixelBgra8(embedPixels, embedXPosition, embedYPosition, embedImageWidth, embedImageHeight);
            Color sourceColor = Color.FromArgb(119, 119, 119, 119);

            if (embedImageWidth > sourceImageWidth || embedImageHeight > sourceImageHeight)
            {
                return;
            }

            for (int y = 0; y < sourceImageHeight; y++)
            {
                for (int x = 0; x < sourceImageWidth; x++)
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
                        if (embedXPosition >= sourceImageWidth || embedYPosition >= sourceImageHeight)
                        {
                            embedColor = Colors.Black;
                        }

                        if (embedColor == Colors.Black)
                        {
                            sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (0 << 0);
                            SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                        else if (embedColor == Colors.White)
                        {
                            sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (1 << 0);
                            SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                    }
                }
            }
        }

        public static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private static void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        public static Color GetBorW(int pix, uint sourceImageWidth, byte[] imageExtract, int x, int y)
        {
            if (pix == 0)
            {
                SetPixelBgra8(imageExtract, x, y, Color.FromArgb(255, 255, 255, 255), sourceImageWidth, sourceImageWidth);
                return Color.FromArgb(0, 0, 0, 0);

            }
            SetPixelBgra8(imageExtract, x, y, Color.FromArgb(255, 255, 255, 255), sourceImageWidth, sourceImageWidth);
            return Color.FromArgb(255, 255, 255, 255);
        }

        private static byte[] ExtractImageWithImage(byte[] sourcePixels, uint sourceImageWidth, uint sourceImageHeight)
        {
            byte[] imageExtract = new byte[sourcePixels.Length];
            int y;
            int x = y = 0;
            Color sourceColor;

            for (var i = 0; i < sourceImageHeight; i++)
            {
                for (var j = 0; j < sourceImageWidth; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        sourceColor = ImageEncryption.GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        if (!sourceColor.Equals(Color.FromArgb(160, 160, 160, 160)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        int pix0 = sourceColor.R & 1;
                        var color = ImageEncryption.GetBorW(pix0, sourceImageWidth, imageExtract, x, y);
                        ImageEncryption.SetPixelBgra8(imageExtract, x, y, color, sourceImageWidth, sourceImageHeight);
                        int pix1 = sourceColor.G & 1;
                        if (x < sourceImageWidth)
                        {
                            x++;
                        }
                        else
                        {
                            x = 0;
                        }

                        if (y < sourceImageHeight)
                        {
                            y++;
                        }
                        else
                        {
                            y = 0;
                        }


                        color = ImageEncryption.GetBorW(pix1, sourceImageWidth, imageExtract, x, y);
                        ImageEncryption.SetPixelBgra8(imageExtract, x, y, color, sourceImageWidth, sourceImageHeight);
                        int pix2 = sourceColor.B & 1;
                        if (x < sourceImageWidth)
                        {
                            x++;
                        }
                        else
                        {
                            x = 0;
                        }

                        if (y < sourceImageHeight)
                        {
                            y++;
                        }
                        else
                        {
                            y = 0;
                        }
                        color = GetBorW(pix2, sourceImageWidth, imageExtract, x, y);
                        SetPixelBgra8(imageExtract, x, y, color, sourceImageWidth, sourceImageHeight);
                        if (x < sourceImageWidth)
                        {
                            x++;
                        }
                        else
                        {
                            x = 0;
                        }

                        if (y < sourceImageHeight)
                        {
                            y++;
                        }
                        else
                        {
                            y = 0;
                        }
                    }
                }
            }

            return imageExtract;

        }
    }
}
