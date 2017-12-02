using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities.Converter
{
    public static class ToImageConverter
    {
        public static async Task<Image> Convert(StorageFile imageFile, Image originalImage)
        {
            var copyBitmapImage = await makeACopyOfTheFileToWorkOn(imageFile);
            using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = System.Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = System.Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                var bitmap = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                var convertedImage = await Convert(sourcePixels, originalImage, bitmap);
                return convertedImage;
            }
        }

        public static async Task<Image> Convert(byte[] sourcePixels, Image originalImage, WriteableBitmap bitmap)
        {
            using (var writeStream = bitmap.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                originalImage.Source = bitmap;
                return originalImage;
            }
        }

        private static async Task<BitmapImage> makeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }
    }
}