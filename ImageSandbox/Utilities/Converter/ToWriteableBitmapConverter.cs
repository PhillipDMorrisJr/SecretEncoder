using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities.Converter
{
    public static class ToWriteableBitmapConverter
    {
        /// <summary>
        ///     Converts to writeable bitmap.
        /// </summary>
        /// <param name="imageToConvert">The image to convert.</param>
        /// <returns></returns>
        public static WriteableBitmap ConvertToWriteableBitmap(Image imageToConvert)
        {
            var writeableBitmap = imageToConvert.Source as WriteableBitmap;

            return writeableBitmap;
        }
    }
}