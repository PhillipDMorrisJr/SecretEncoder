using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities.Converter
{
    public static class WriteableBitmapConverter
    {
        /// <summary>
        /// Converts to writeable bitmap.
        /// </summary>
        /// <param name="imageToConvert">The image to convert.</param>
        /// <returns></returns>
        public static WriteableBitmap ConvertToWriteableBitmap(Image imageToConvert)
        {
            WriteableBitmap writeableBitmap = imageToConvert.Source as WriteableBitmap;
            
            return writeableBitmap;
        }

    }
}
