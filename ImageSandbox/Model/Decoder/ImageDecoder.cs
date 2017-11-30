using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace ImageSandbox.Model.Decoder
{
    public static class ImageDecoder
    {
        public static void DecodeImage(byte[] sourcePixels, uint sourceImageWidth, uint sourceImageHeight)
        {

            /*Color sourceColor = Color.FromArgb(119, 119, 119, 119);
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
                        else
                        {
                            sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                            sourceColor.B |= (1 << 0);
                            SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        }
                    }
                }
            }*/
        }
    }
}
