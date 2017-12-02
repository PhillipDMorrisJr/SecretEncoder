using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Utilities
{
    public static class PixelRetriever
    {
        public static Color RetrieveColor(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            if (offset + 2 >= pixels.Length)
                return Colors.Black;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        public static void ModifyPixel(byte[] pixels, int x, int y, Color color, uint width)
        {
            var offset = (x * (int)width + y) * 4;
            if (offset + 2 >= pixels.Length)
                return;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }
    }
}
