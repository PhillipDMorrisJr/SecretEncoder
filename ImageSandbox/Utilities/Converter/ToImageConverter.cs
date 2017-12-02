using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities
{
    public static class ToImageConverter
    {

        public static async Task<Image> Convert(StorageFile imageFile, Image originalImage)
        {
            WriteableBitmap bitmap;
            var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(imageFile);
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

                bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                Image convertedImage = await Convert(sourcePixels, originalImage, bitmap);
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
        private static async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }
    }
}
